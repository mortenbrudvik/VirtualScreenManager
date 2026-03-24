using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using VirtualDisplayDriver;
using VirtualScreenManager.Core.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace VirtualScreenManager.UI.ViewModels;

public partial class StatusViewModel : ViewModelBase
{
    private readonly IVirtualDisplayManager _displayManager;
    private readonly IVirtualDisplaySetup _displaySetup;
    private readonly IContentDialogService _contentDialogService;
    private readonly ISnackbarService _snackbarService;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<StatusViewModel> _logger;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallDriverCommand))]
    [NotifyCanExecuteChangedFor(nameof(UninstallDriverCommand))]
    [NotifyCanExecuteChangedFor(nameof(EnableDriverCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisableDriverCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestartDeviceCommand))]
    [NotifyPropertyChangedFor(nameof(IsDeviceDisabled))]
    private bool _isDriverInstalled;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isPipeRunning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeviceDisabled))]
    [NotifyCanExecuteChangedFor(nameof(EnableDriverCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisableDriverCommand))]
    private bool _isDeviceEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeviceDisabled))]
    [NotifyCanExecuteChangedFor(nameof(RestartDeviceCommand))]
    private bool _hasDeviceError;

    public bool IsDeviceDisabled => IsDriverInstalled && !IsDeviceEnabled && !HasDeviceError;

    [ObservableProperty]
    private int _displayCount;

    [ObservableProperty]
    private string _deviceStateText = "Unknown";

    [ObservableProperty]
    private bool _isRefreshing;

    public StatusViewModel(
        IVirtualDisplayManager displayManager,
        IVirtualDisplaySetup displaySetup,
        IContentDialogService contentDialogService,
        ISnackbarService snackbarService,
        IActivityLogger activityLogger,
        ILogger<StatusViewModel> logger)
    {
        _displayManager = displayManager;
        _displaySetup = displaySetup;
        _contentDialogService = contentDialogService;
        _snackbarService = snackbarService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public override async Task OnNavigatedToAsync()
    {
        await RefreshStatusAsync();
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsDriverInstalled = deviceState != DeviceState.NotFound;
                IsDeviceEnabled = deviceState == DeviceState.Enabled;
                HasDeviceError = deviceState == DeviceState.Error;
                DeviceStateText = deviceState.ToString();
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
                            displayCount = VirtualDisplayConfiguration.GetVirtualMonitors().Count;
                            break;
                        }
                    }
                    catch
                    {
                        // Pipe not ready yet
                    }

                    await Task.Delay(1000).ConfigureAwait(false);
                }

                if (!isConnected)
                {
                    _logger.LogWarning("Driver pipe not responding after retries");
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsConnected = isConnected;
                IsPipeRunning = isConnected;
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
            Application.Current.Dispatcher.Invoke(() => IsRefreshing = false);
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
            Application.Current.Dispatcher.Invoke(() =>
                _snackbarService.Show("Success", "Virtual Display Driver installed successfully.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3)));
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Driver installation failed");
            _activityLogger.Error("Setup", "Driver installation failed", ex.Message);
            Application.Current.Dispatcher.Invoke(() =>
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
            Application.Current.Dispatcher.Invoke(() =>
                _snackbarService.Show("Success", "Driver uninstalled successfully.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3)));
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Driver uninstall failed");
            _activityLogger.Error("Setup", "Uninstall failed", ex.Message);
            Application.Current.Dispatcher.Invoke(() =>
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
        }
    }
}
