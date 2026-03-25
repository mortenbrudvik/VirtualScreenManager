using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using VirtualDisplayDriver;
using VirtualScreenManager.Core.Services;
using VirtualScreenManager.UI.Services;
using VirtualScreenManager.UI.ViewModels;
using Wpf.Ui;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.ViewModels;

public class StatusViewModelTests
{
    private readonly IVirtualDisplayManager _displayManager = Substitute.For<IVirtualDisplayManager>();
    private readonly IVirtualDisplaySetup _displaySetup = Substitute.For<IVirtualDisplaySetup>();
    private readonly IVirtualDisplayInfo _displayInfo = Substitute.For<IVirtualDisplayInfo>();
    private readonly IContentDialogService _contentDialogService = Substitute.For<IContentDialogService>();
    private readonly ISnackbarService _snackbarService = Substitute.For<ISnackbarService>();
    private readonly IActivityLogger _activityLogger = Substitute.For<IActivityLogger>();
    private readonly IDispatcherService _dispatcher = Substitute.For<IDispatcherService>();
    private readonly ILogger<StatusViewModel> _logger = Substitute.For<ILogger<StatusViewModel>>();
    private readonly StatusViewModel _sut;

    public StatusViewModelTests()
    {
        _dispatcher.When(x => x.Invoke(Arg.Any<Action>()))
            .Do(x => x.Arg<Action>()());

        _displayInfo.GetVirtualMonitors().Returns([]);

        _sut = new StatusViewModel(
            _displayManager, _displaySetup, _displayInfo,
            _contentDialogService, _snackbarService, _activityLogger,
            _dispatcher, _logger);
    }

    [Fact]
    public void CanInstallDriver_WhenNotInstalled_ReturnsTrue()
    {
        _sut.CurrentDeviceState.ShouldBe(DeviceState.NotFound);
        _sut.InstallDriverCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshStatusAsync_WhenEnabled_UpdatesConnection()
    {
        _displaySetup.GetDeviceStateAsync().Returns(DeviceState.Enabled);
        _displayManager.PingAsync().Returns(true);

        await _sut.RefreshStatusCommand.ExecuteAsync(null);

        _sut.CurrentDeviceState.ShouldBe(DeviceState.Enabled);
        _sut.IsConnected.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshStatusAsync_WhenNotFound_ShowsNotInstalled()
    {
        _displaySetup.GetDeviceStateAsync().Returns(DeviceState.NotFound);

        await _sut.RefreshStatusCommand.ExecuteAsync(null);

        _sut.CurrentDeviceState.ShouldBe(DeviceState.NotFound);
        _sut.IsDriverInstalled.ShouldBeFalse();
        _sut.IsConnected.ShouldBeFalse();
    }

    [Fact]
    public void IsDriverInstalled_WhenNotFound_ReturnsFalse()
    {
        _sut.IsDriverInstalled.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshStatusAsync_SetsIsRefreshing()
    {
        var wasRefreshing = false;
        _sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(StatusViewModel.IsRefreshing) && _sut.IsRefreshing)
                wasRefreshing = true;
        };

        _displaySetup.GetDeviceStateAsync().Returns(DeviceState.NotFound);

        await _sut.RefreshStatusCommand.ExecuteAsync(null);

        wasRefreshing.ShouldBeTrue();
        _sut.IsRefreshing.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshStatusAsync_WhenError_LogsAndDoesNotThrow()
    {
        _displaySetup.GetDeviceStateAsync().ThrowsAsync(new InvalidOperationException("test"));

        await _sut.RefreshStatusCommand.ExecuteAsync(null);

        _activityLogger.Received(1).Error("Status", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void CanExecute_Guards_ReflectDeviceState()
    {
        _sut.InstallDriverCommand.CanExecute(null).ShouldBeTrue();
        _sut.UninstallDriverCommand.CanExecute(null).ShouldBeFalse();
        _sut.EnableDriverCommand.CanExecute(null).ShouldBeFalse();
        _sut.DisableDriverCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void DeviceStateText_ReturnsEnumString()
    {
        _sut.DeviceStateText.ShouldBe(DeviceState.NotFound.ToString());
    }

    [Fact]
    public void HasDeviceError_WhenNotError_ReturnsFalse()
    {
        _sut.HasDeviceError.ShouldBeFalse();
    }
}
