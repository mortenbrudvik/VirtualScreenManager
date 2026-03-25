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

public class DisplayManagementViewModelTests
{
    private readonly IVirtualDisplayManager _displayManager = Substitute.For<IVirtualDisplayManager>();
    private readonly IVirtualDisplaySetup _displaySetup = Substitute.For<IVirtualDisplaySetup>();
    private readonly IVirtualDisplayInfo _displayInfo = Substitute.For<IVirtualDisplayInfo>();
    private readonly ISnackbarService _snackbarService = Substitute.For<ISnackbarService>();
    private readonly IActivityLogger _activityLogger = Substitute.For<IActivityLogger>();
    private readonly IDispatcherService _dispatcher = Substitute.For<IDispatcherService>();
    private readonly ILogger<DisplayManagementViewModel> _logger = Substitute.For<ILogger<DisplayManagementViewModel>>();
    private readonly DisplayManagementViewModel _sut;

    public DisplayManagementViewModelTests()
    {
        _dispatcher.When(x => x.Invoke(Arg.Any<Action>()))
            .Do(x => x.Arg<Action>()());

        _displayInfo.GetVirtualMonitors().Returns([]);
        _displayInfo.GetAllMonitors().Returns([]);

        _sut = new DisplayManagementViewModel(
            _displayManager, _displaySetup, _displayInfo,
            _snackbarService, _activityLogger, _dispatcher, _logger);
    }

    [Fact]
    public void DisplayCount_Validation_ClampsToMax()
    {
        _sut.DisplayCount = 20;
        _sut.DisplayCount.ShouldBe(16);
    }

    [Fact]
    public void DisplayCount_Validation_ClampsToMin()
    {
        _sut.DisplayCount = 0;
        _sut.DisplayCount.ShouldBe(1);
    }

    [Fact]
    public void DisplayCount_Validation_ClampsNegative()
    {
        _sut.DisplayCount = -5;
        _sut.DisplayCount.ShouldBe(1);
    }

    [Fact]
    public void DisplayCount_Validation_RoundsUp()
    {
        _sut.DisplayCount = 2.7;
        _sut.DisplayCount.ShouldBe(3);
    }

    [Fact]
    public void DisplayCount_Validation_RoundsDown()
    {
        _sut.DisplayCount = 2.3;
        _sut.DisplayCount.ShouldBe(2);
    }

    [Fact]
    public void DisplayCount_ValidValue_DoesNotClamp()
    {
        _sut.DisplayCount = 8;
        _sut.DisplayCount.ShouldBe(8);
    }

    [Fact]
    public async Task SetDisplayCountAsync_CallsDriverPipeline()
    {
        _sut.DisplayCount = 3;

        await _sut.SetDisplayCountCommand.ExecuteAsync(null);

        _displayInfo.Received(1).SetConfiguredDisplayCount(3);
        await _displayManager.Received(1).SyncDisplayCountAsync(3);
        await _displayManager.Received(1).SetDisplayCountAsync(3);
    }

    [Fact]
    public async Task SetDisplayCountAsync_OnFailure_ShowsError()
    {
        _sut.DisplayCount = 2;
        _displayManager.PingAsync().ThrowsAsync(new Exception("not connected"));

        await _sut.SetDisplayCountCommand.ExecuteAsync(null);

        _activityLogger.Received(1).Error("Displays", Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task RefreshAsync_LoadsMonitors()
    {
        _displayManager.PingAsync().ThrowsAsync(new Exception("no pipe"));

        await _sut.RefreshCommand.ExecuteAsync(null);

        _displayInfo.Received().GetAllMonitors();
        _displayInfo.Received().GetVirtualMonitors();
    }

    [Fact]
    public async Task RemoveAllAsync_ClearsDisplays()
    {
        _sut.DisplayCount = 3;

        await _sut.RemoveAllCommand.ExecuteAsync(null);

        await _displayManager.Received(1).RemoveAllDisplaysAsync();
        _sut.VirtualMonitors.Count.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshAsync_SetsIsRefreshing()
    {
        _displayManager.PingAsync().ThrowsAsync(new Exception("no pipe"));

        var wasRefreshing = false;
        _sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DisplayManagementViewModel.IsRefreshing) && _sut.IsRefreshing)
                wasRefreshing = true;
        };

        await _sut.RefreshCommand.ExecuteAsync(null);

        wasRefreshing.ShouldBeTrue();
        _sut.IsRefreshing.ShouldBeFalse();
    }
}
