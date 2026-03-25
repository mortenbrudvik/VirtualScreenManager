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

public class SettingsViewModelTests
{
    private readonly IVirtualDisplayManager _displayManager = Substitute.For<IVirtualDisplayManager>();
    private readonly IVirtualDisplaySetup _displaySetup = Substitute.For<IVirtualDisplaySetup>();
    private readonly IVirtualDisplayInfo _displayInfo = Substitute.For<IVirtualDisplayInfo>();
    private readonly ISnackbarService _snackbarService = Substitute.For<ISnackbarService>();
    private readonly IActivityLogger _activityLogger = Substitute.For<IActivityLogger>();
    private readonly IDispatcherService _dispatcher = Substitute.For<IDispatcherService>();
    private readonly ILogger<SettingsViewModel> _logger = Substitute.For<ILogger<SettingsViewModel>>();
    private readonly SettingsViewModel _sut;

    public SettingsViewModelTests()
    {
        _dispatcher.When(x => x.Invoke(Arg.Any<Action>()))
            .Do(x => x.Arg<Action>()());

        _sut = new SettingsViewModel(
            _displayManager, _displaySetup, _displayInfo,
            _snackbarService, _activityLogger, _dispatcher, _logger);
    }

    [Fact]
    public async Task ToggleHdrPlusAsync_OnSuccess_LogsChange()
    {
        _sut.HdrPlusEnabled = true;

        await _sut.ToggleHdrPlusCommand.ExecuteAsync(null);

        await _displayManager.Received(1).SetHdrPlusAsync(true);
        _activityLogger.Received(1).Info("Settings", Arg.Is<string>(s => s.Contains("HDR+")));
    }

    [Fact]
    public async Task ToggleHdrPlusAsync_OnFailure_RevertsToggle()
    {
        _sut.HdrPlusEnabled = true;
        _displayManager.SetHdrPlusAsync(Arg.Any<bool>()).ThrowsAsync(new Exception("pipe error"));

        await _sut.ToggleHdrPlusCommand.ExecuteAsync(null);

        _sut.HdrPlusEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task ToggleHdrPlusAsync_OnFailure_ShowsSnackbar()
    {
        _sut.HdrPlusEnabled = true;
        _displayManager.SetHdrPlusAsync(Arg.Any<bool>()).ThrowsAsync(new Exception("fail"));

        await _sut.ToggleHdrPlusCommand.ExecuteAsync(null);

        _snackbarService.Received(1).Show(
            Arg.Any<string>(), Arg.Is<string>(s => s.Contains("fail")),
            Arg.Any<Wpf.Ui.Controls.ControlAppearance>(),
            Arg.Any<Wpf.Ui.Controls.IconElement?>(),
            Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task ExecuteSettingChangeAsync_DriverReloadSetting_ChecksDeviceState()
    {
        _sut.CustomEdidEnabled = true;
        _displaySetup.GetDeviceStateAsync().Returns(DeviceState.Enabled);

        await _sut.ToggleCustomEdidCommand.ExecuteAsync(null);

        await _displaySetup.Received(1).GetDeviceStateAsync();
    }

    [Fact]
    public async Task ExecuteSettingChangeAsync_DriverReloadError_RestartsDevice()
    {
        _sut.CustomEdidEnabled = true;
        _displaySetup.GetDeviceStateAsync().Returns(DeviceState.Error);

        await _sut.ToggleCustomEdidCommand.ExecuteAsync(null);

        await _displaySetup.Received(1).RestartDeviceAsync();
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenConnected_LoadsSettings()
    {
        _displayManager.PingAsync().Returns(true);
        _displayManager.IsConnected.Returns(true);
        _displayManager.GetSettingsAsync().Returns(new DriverSettings(
            HdrPlus: true, Sdr10Bit: false, CustomEdid: true, PreventSpoof: false,
            CeaOverride: true, HardwareCursor: false, DebugLogging: true, Logging: false));
        _displayManager.GetAssignedGpuAsync().Returns("GPU 0");
        _displayManager.GetAllGpusAsync().Returns(new[] { "GPU 0", "GPU 1" });
        _displayInfo.GetInstallPath().Returns(@"C:\Drivers\VDD");

        await _sut.OnNavigatedToAsync();

        _sut.HdrPlusEnabled.ShouldBeTrue();
        _sut.CustomEdidEnabled.ShouldBeTrue();
        _sut.CeaOverrideEnabled.ShouldBeTrue();
        _sut.DebugLoggingEnabled.ShouldBeTrue();
        _sut.CurrentGpu.ShouldBe("GPU 0");
        _sut.AvailableGpus.Count.ShouldBe(2);
        _sut.InstallPath.ShouldBe(@"C:\Drivers\VDD");
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenNotConnected_OnlyLoadsInstallPath()
    {
        _displayManager.PingAsync().ThrowsAsync(new Exception("no pipe"));
        _displayManager.IsConnected.Returns(false);
        _displayInfo.GetInstallPath().Returns((string?)null);

        await _sut.OnNavigatedToAsync();

        _sut.InstallPath.ShouldBe("Not installed");
        _sut.HdrPlusEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task ToggleSdr10BitAsync_OnSuccess_SendsToDriver()
    {
        _sut.Sdr10BitEnabled = true;

        await _sut.ToggleSdr10BitCommand.ExecuteAsync(null);

        await _displayManager.Received(1).SetSdr10BitAsync(true);
    }

    [Fact]
    public async Task SetGpuAsync_WhenEmpty_DoesNothing()
    {
        _sut.SelectedGpu = null;

        await _sut.SetGpuCommand.ExecuteAsync(null);

        await _displayManager.DidNotReceive().SetGpuAsync(Arg.Any<string>());
    }
}
