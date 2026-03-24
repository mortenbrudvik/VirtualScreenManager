using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using VirtualDisplayDriver;
using VirtualScreenManager.Core.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace VirtualScreenManager.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IVirtualDisplayManager _displayManager;
    private readonly IVirtualDisplaySetup _displaySetup;
    private readonly ISnackbarService _snackbarService;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<SettingsViewModel> _logger;

    // Display Features
    [ObservableProperty]
    private bool _hdrPlusEnabled;

    [ObservableProperty]
    private bool _sdr10BitEnabled;

    [ObservableProperty]
    private bool _customEdidEnabled;

    // Advanced
    [ObservableProperty]
    private bool _preventSpoofEnabled;

    [ObservableProperty]
    private bool _ceaOverrideEnabled;

    // Cursor
    [ObservableProperty]
    private bool _hardwareCursorEnabled;

    // Diagnostics
    [ObservableProperty]
    private bool _debugLoggingEnabled;

    [ObservableProperty]
    private bool _loggingEnabled;

    // GPU
    [ObservableProperty]
    private string _currentGpu = string.Empty;

    [ObservableProperty]
    private string? _selectedGpu;

    public ObservableCollection<string> AvailableGpus { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _installPath = "Not installed";

    public SettingsViewModel(
        IVirtualDisplayManager displayManager,
        IVirtualDisplaySetup displaySetup,
        ISnackbarService snackbarService,
        IActivityLogger activityLogger,
        ILogger<SettingsViewModel> logger)
    {
        _displayManager = displayManager;
        _displaySetup = displaySetup;
        _snackbarService = snackbarService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public override async Task OnNavigatedToAsync()
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        IsLoading = true;
        try
        {
            // Load install path
            var path = VirtualDisplayDetection.GetInstallPath();
            Application.Current.Dispatcher.Invoke(() =>
                InstallPath = path ?? "Not installed");

            // Ensure pipe connection
            try
            {
                await _displayManager.PingAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Pipe not connected — settings unavailable");
            }

            // Load driver settings via pipe
            if (_displayManager.IsConnected)
            {
                var settings = await _displayManager.GetSettingsAsync().ConfigureAwait(false);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HdrPlusEnabled = settings.HdrPlus;
                    Sdr10BitEnabled = settings.Sdr10Bit;
                    CustomEdidEnabled = settings.CustomEdid;
                    PreventSpoofEnabled = settings.PreventSpoof;
                    CeaOverrideEnabled = settings.CeaOverride;
                    HardwareCursorEnabled = settings.HardwareCursor;
                    DebugLoggingEnabled = settings.DebugLogging;
                    LoggingEnabled = settings.Logging;
                });
            }

            // Load GPU info
            if (_displayManager.IsConnected)
            {
                try
                {
                    var currentGpu = await _displayManager.GetAssignedGpuAsync().ConfigureAwait(false);
                    var allGpus = await _displayManager.GetAllGpusAsync().ConfigureAwait(false);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentGpu = currentGpu;
                        AvailableGpus.Clear();
                        foreach (var gpu in allGpus)
                        {
                            AvailableGpus.Add(gpu);
                        }
                        SelectedGpu = currentGpu;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load GPU info");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            _activityLogger.Error("Settings", "Failed to load settings", ex.Message);
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => IsLoading = false);
        }
    }

    [RelayCommand]
    private async Task ToggleHdrPlusAsync()
    {
        await ExecuteSettingChangeAsync(
            "HDR+", HdrPlusEnabled,
            value => _displayManager.SetHdrPlusAsync(value),
            value => HdrPlusEnabled = value);
    }

    [RelayCommand]
    private async Task ToggleSdr10BitAsync()
    {
        await ExecuteSettingChangeAsync(
            "SDR 10-bit", Sdr10BitEnabled,
            value => _displayManager.SetSdr10BitAsync(value),
            value => Sdr10BitEnabled = value);
    }

    [RelayCommand]
    private async Task ToggleCustomEdidAsync()
    {
        await ExecuteSettingChangeAsync(
            "Custom EDID", CustomEdidEnabled,
            value => _displayManager.SetCustomEdidAsync(value),
            value => CustomEdidEnabled = value);
    }

    [RelayCommand]
    private async Task TogglePreventSpoofAsync()
    {
        await ExecuteSettingChangeAsync(
            "Prevent Spoof", PreventSpoofEnabled,
            value => _displayManager.SetPreventSpoofAsync(value),
            value => PreventSpoofEnabled = value);
    }

    [RelayCommand]
    private async Task ToggleCeaOverrideAsync()
    {
        await ExecuteSettingChangeAsync(
            "CEA Override", CeaOverrideEnabled,
            value => _displayManager.SetCeaOverrideAsync(value),
            value => CeaOverrideEnabled = value);
    }

    [RelayCommand]
    private async Task ToggleHardwareCursorAsync()
    {
        await ExecuteSettingChangeAsync(
            "Hardware Cursor", HardwareCursorEnabled,
            value => _displayManager.SetHardwareCursorAsync(value),
            value => HardwareCursorEnabled = value);
    }

    [RelayCommand]
    private async Task ToggleDebugLoggingAsync()
    {
        await ExecuteSettingChangeAsync(
            "Debug Logging", DebugLoggingEnabled,
            value => _displayManager.SetDebugLoggingAsync(value),
            value => DebugLoggingEnabled = value);
    }

    [RelayCommand]
    private async Task ToggleLoggingAsync()
    {
        await ExecuteSettingChangeAsync(
            "Logging", LoggingEnabled,
            value => _displayManager.SetLoggingAsync(value),
            value => LoggingEnabled = value);
    }

    [RelayCommand]
    private async Task SetGpuAsync()
    {
        if (string.IsNullOrEmpty(SelectedGpu)) return;

        try
        {
            await _displayManager.SetGpuAsync(SelectedGpu).ConfigureAwait(false);
            _activityLogger.Info("Settings", $"GPU changed to {SelectedGpu}");

            Application.Current.Dispatcher.Invoke(() => CurrentGpu = SelectedGpu);

            Application.Current.Dispatcher.Invoke(() =>
                _snackbarService.Show("GPU Updated", $"GPU set to {SelectedGpu}", ControlAppearance.Success, null, TimeSpan.FromSeconds(3)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set GPU");
            _activityLogger.Error("Settings", "Failed to set GPU", ex.Message);
            Application.Current.Dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Failed to set GPU: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
    }

    [RelayCommand]
    private void OpenInstallFolder()
    {
        var path = VirtualDisplayDetection.GetInstallPath();
        if (path is not null && Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo(path!) { UseShellExecute = true });
        }
    }

    private async Task ExecuteSettingChangeAsync(string settingName, bool value, Func<bool, Task> action, Action<bool> revert)
    {
        try
        {
            await action(value).ConfigureAwait(false);
            _activityLogger.Info("Settings", $"{settingName} {(value ? "enabled" : "disabled")}");

            // Auto-recovery for settings that trigger driver reload
            await Task.Delay(2000).ConfigureAwait(false);
            var deviceState = await _displaySetup.GetDeviceStateAsync().ConfigureAwait(false);
            if (deviceState == DeviceState.Error)
            {
                _activityLogger.Warning("Settings", $"Driver crash after {settingName} change, restarting...");
                await _displaySetup.RestartDeviceAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change {Setting}", settingName);
            _activityLogger.Error("Settings", $"Failed to change {settingName}", ex.Message);
            Application.Current.Dispatcher.Invoke(() =>
            {
                revert(!value);
                _snackbarService.Show("Error", $"Failed to change {settingName}: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5));
            });
        }
    }
}
