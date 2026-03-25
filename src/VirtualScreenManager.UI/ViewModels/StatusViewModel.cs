using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using VirtualDisplayDriver;
using VirtualScreenManager.Core.Services;
using VirtualScreenManager.UI.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace VirtualScreenManager.UI.ViewModels;

public partial class StatusViewModel : ViewModelBase
{
    private readonly IVirtualDisplayManager _displayManager;
    private readonly IVirtualDisplaySetup _displaySetup;
    private readonly IVirtualDisplayInfo _displayInfo;
    private readonly IContentDialogService _contentDialogService;
    private readonly ISnackbarService _snackbarService;
    private readonly IActivityLogger _activityLogger;
    private readonly IDispatcherService _dispatcher;
    private readonly ILogger<StatusViewModel> _logger;

    private DeviceState _currentDeviceState;

    public DeviceState CurrentDeviceState
    {
        get => _currentDeviceState;
        private set
        {
            if (SetProperty(ref _currentDeviceState, value))
            {
                OnPropertyChanged(nameof(IsDriverInstalled));
                OnPropertyChanged(nameof(IsDeviceEnabled));
                OnPropertyChanged(nameof(HasDeviceError));
                OnPropertyChanged(nameof(IsDeviceDisabled));
                OnPropertyChanged(nameof(DeviceStateText));
                InstallDriverCommand.NotifyCanExecuteChanged();
                UninstallDriverCommand.NotifyCanExecuteChanged();
                EnableDriverCommand.NotifyCanExecuteChanged();
                DisableDriverCommand.NotifyCanExecuteChanged();
                RestartDeviceCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsDriverInstalled => CurrentDeviceState != DeviceState.NotFound;
    public bool IsDeviceEnabled => CurrentDeviceState == DeviceState.Enabled;
    public bool HasDeviceError => CurrentDeviceState == DeviceState.Error;
    public bool IsDeviceDisabled => IsDriverInstalled && !IsDeviceEnabled && !HasDeviceError;
    public string DeviceStateText => CurrentDeviceState.ToString();

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private int _displayCount;

    [ObservableProperty]
    private bool _isRefreshing;

    public StatusViewModel(
        IVirtualDisplayManager displayManager,
        IVirtualDisplaySetup displaySetup,
        IVirtualDisplayInfo displayInfo,
        IContentDialogService contentDialogService,
        ISnackbarService snackbarService,
        IActivityLogger activityLogger,
        IDispatcherService dispatcher,
        ILogger<StatusViewModel> logger)
    {
        _displayManager = displayManager;
        _displaySetup = displaySetup;
        _displayInfo = displayInfo;
        _contentDialogService = contentDialogService;
        _snackbarService = snackbarService;
        _activityLogger = activityLogger;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public override async Task OnNavigatedToAsync()
    {
        await RefreshStatusAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        IsRefreshing = true;
        try
        {
            var deviceState = await _displaySetup.GetDeviceStateAsync().ConfigureAwait(false);
            var isConnected = false;
            int displayCount = 0;

            _dispatcher.Invoke(() =>
            {
                CurrentDeviceState = deviceState;
            });

            if (deviceState == DeviceState.Enabled)
            {
                // Retry ping a few times — driver pipe may need time to initialize after install/restart
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        isConnected = await _displayManager.PingAsync().ConfigureAwait(false);
                        if (isConnected)
                        {
                            displayCount = _displayInfo.GetVirtualMonitors().Count;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Ping attempt {Attempt} failed", attempt + 1);
                    }

                    await Task.Delay(1000).ConfigureAwait(false);
                }

                if (!isConnected)
                {
                    _logger.LogWarning("Driver pipe not responding after retries");
                }
            }

            _dispatcher.Invoke(() =>
            {
                IsConnected = isConnected;
                DisplayCount = displayCount;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh status");
            _activityLogger.Error("Status", "Failed to refresh status", ex.Message);
        }
        finally
        {
            _dispatcher.Invoke(() => IsRefreshing = false);
        }
    }

    private bool CanInstallDriver() => !IsDriverInstalled;

    [RelayCommand(CanExecute = nameof(CanInstallDriver))]
    private async Task InstallDriverAsync()
    {
        _activityLogger.Info("Setup", "Starting driver installation...");

        try
        {
            await _displaySetup.InstallDriverAsync().ConfigureAwait(false);
            _activityLogger.Info("Setup", "Driver installed successfully");
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Success", "Virtual Display Driver installed successfully.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3)));
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Driver installation failed");
            _activityLogger.Error("Setup", "Driver installation failed", ex.Message);
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Installation failed: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
    }

    private bool CanUninstallDriver() => IsDriverInstalled;

    [RelayCommand(CanExecute = nameof(CanUninstallDriver))]
    private async Task UninstallDriverAsync()
    {
        try
        {
            _activityLogger.Info("Setup", "Uninstalling driver...");
            await _displaySetup.UninstallDriverAsync().ConfigureAwait(false);
            _activityLogger.Info("Setup", "Driver uninstalled successfully");
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Success", "Driver uninstalled successfully.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3)));
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Driver uninstall failed");
            _activityLogger.Error("Setup", "Uninstall failed", ex.Message);
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Uninstall failed: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
    }

    private bool CanEnableDriver() => IsDriverInstalled && !IsDeviceEnabled;

    [RelayCommand(CanExecute = nameof(CanEnableDriver))]
    private async Task EnableDriverAsync()
    {
        try
        {
            await _displaySetup.EnableDeviceAsync().ConfigureAwait(false);
            _activityLogger.Info("Device", "Driver enabled");
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable driver");
            _activityLogger.Error("Device", "Failed to enable driver", ex.Message);
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Failed to enable driver: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
    }

    private bool CanDisableDriver() => IsDriverInstalled && IsDeviceEnabled;

    [RelayCommand(CanExecute = nameof(CanDisableDriver))]
    private async Task DisableDriverAsync()
    {
        try
        {
            await _displaySetup.DisableDeviceAsync().ConfigureAwait(false);
            _activityLogger.Info("Device", "Driver disabled");
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable driver");
            _activityLogger.Error("Device", "Failed to disable driver", ex.Message);
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Failed to disable driver: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
    }

    private bool CanRestartDevice() => IsDriverInstalled && (HasDeviceError || IsDeviceEnabled);

    [RelayCommand(CanExecute = nameof(CanRestartDevice))]
    private async Task RestartDeviceAsync()
    {
        try
        {
            _activityLogger.Info("Device", "Restarting device...");
            await _displaySetup.RestartDeviceAsync().ConfigureAwait(false);
            _activityLogger.Info("Device", "Device restarted");
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart device");
            _activityLogger.Error("Device", "Failed to restart device", ex.Message);
            _dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Failed to restart device: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
    }
}
