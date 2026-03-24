using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using VirtualDisplayDriver;
using VirtualScreenManager.Core.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace VirtualScreenManager.UI.ViewModels;

public partial class DisplayManagementViewModel : ViewModelBase
{
    private readonly IVirtualDisplayManager _displayManager;
    private readonly IVirtualDisplaySetup _displaySetup;
    private readonly ISnackbarService _snackbarService;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<DisplayManagementViewModel> _logger;

    [ObservableProperty]
    private double? _displayCount = 1;

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private bool _isRefreshing;

    public ObservableCollection<SystemMonitor> AllMonitors { get; } = [];
    public ObservableCollection<VirtualMonitor> VirtualMonitors { get; } = [];

    public DisplayManagementViewModel(
        IVirtualDisplayManager displayManager,
        IVirtualDisplaySetup displaySetup,
        ISnackbarService snackbarService,
        IActivityLogger activityLogger,
        ILogger<DisplayManagementViewModel> logger)
    {
        _displayManager = displayManager;
        _displaySetup = displaySetup;
        _snackbarService = snackbarService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public override async Task OnNavigatedToAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            // Ensure pipe connection and sync count from XML
            try
            {
                await _displayManager.PingAsync().ConfigureAwait(false);
                var xmlCount = VirtualDisplayDetection.GetConfiguredDisplayCount();
                if (xmlCount > 0)
                {
                    await _displayManager.SyncDisplayCountAsync(xmlCount).ConfigureAwait(false);
                }
            }
            catch
            {
                // Not connected
            }

            var virtualMonitors = VirtualDisplayConfiguration.GetVirtualMonitors();
            Application.Current.Dispatcher.Invoke(() =>
            {
                DisplayCount = virtualMonitors.Count > 0 ? virtualMonitors.Count : 1;
            });

            await RefreshMonitorListAsync();
            _activityLogger.Info("Displays", $"Refreshed — {AllMonitors.Count} display(s) detected");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh display info");
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => IsRefreshing = false);
        }
    }

    [RelayCommand]
    private async Task SetDisplayCountAsync()
    {
        var targetCount = (int)(DisplayCount ?? 1);
        if (targetCount < 1 || targetCount > 16) return;

        IsUpdating = true;
        try
        {

            // Write count to XML first (driver reads from XML on reload)
            VirtualDisplayDetection.SetConfiguredDisplayCount(targetCount);

            // Ensure pipe connection, sync, and send command
            await _displayManager.PingAsync().ConfigureAwait(false);
            await _displayManager.SyncDisplayCountAsync(targetCount).ConfigureAwait(false);
            await _displayManager.SetDisplayCountAsync(targetCount).ConfigureAwait(false);

            _activityLogger.Info("Displays", $"Display count set to {targetCount}");

            // Wait for driver to stabilize
            await Task.Delay(2000).ConfigureAwait(false);

            // Auto-recovery: check for driver crash
            await AutoRecoverIfNeededAsync();
            await RefreshMonitorListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set display count");
            _activityLogger.Error("Displays", "Failed to set display count", ex.Message);
            Application.Current.Dispatcher.Invoke(() =>
                _snackbarService.Show("Error", $"Failed to set display count: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5)));
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => IsUpdating = false);
        }
    }

    [RelayCommand]
    private async Task RemoveAllAsync()
    {
        IsUpdating = true;
        try
        {
            await _displayManager.RemoveAllDisplaysAsync().ConfigureAwait(false);
            _activityLogger.Info("Displays", "All virtual displays removed");

            Application.Current.Dispatcher.Invoke(() =>
            {
                DisplayCount = 0;
                VirtualMonitors.Clear();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove all displays");
            _activityLogger.Error("Displays", "Failed to remove all displays", ex.Message);
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => IsUpdating = false);
        }
    }

    [RelayCommand]
    private void OpenWindowsDisplaySettings()
    {
        Process.Start(new ProcessStartInfo("ms-settings:display") { UseShellExecute = true });
    }

    private async Task AutoRecoverIfNeededAsync()
    {
        try
        {
            var deviceState = await _displaySetup.GetDeviceStateAsync().ConfigureAwait(false);
            if (deviceState == DeviceState.Error)
            {
                _activityLogger.Warning("Displays", "Driver crash detected (Code 43), attempting auto-recovery...");
                await _displaySetup.RestartDeviceAsync().ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
                _activityLogger.Info("Displays", "Auto-recovery completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-recovery failed");
            _activityLogger.Error("Displays", "Auto-recovery failed", ex.Message);
        }
    }

    private Task RefreshMonitorListAsync()
    {
        try
        {
            var allMonitors = VirtualDisplayConfiguration.GetAllMonitors();
            var virtualMonitors = VirtualDisplayConfiguration.GetVirtualMonitors();

            Application.Current.Dispatcher.Invoke(() =>
            {
                AllMonitors.Clear();
                foreach (var monitor in allMonitors)
                {
                    AllMonitors.Add(monitor);
                }

                VirtualMonitors.Clear();
                foreach (var monitor in virtualMonitors)
                {
                    VirtualMonitors.Add(monitor);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh monitor list");
        }

        return Task.CompletedTask;
    }
}
