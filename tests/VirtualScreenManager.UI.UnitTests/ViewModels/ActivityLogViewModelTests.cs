using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using VirtualScreenManager.Core.Models;
using VirtualScreenManager.Core.Services;
using VirtualScreenManager.UI.Services;
using VirtualScreenManager.UI.ViewModels;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.ViewModels;

public class ActivityLogViewModelTests
{
    private readonly IActivityLogger _activityLogger = Substitute.For<IActivityLogger>();
    private readonly IDispatcherService _dispatcher = Substitute.For<IDispatcherService>();
    private readonly ActivityLogViewModel _sut;

    public ActivityLogViewModelTests()
    {
        // Make the dispatcher execute actions immediately for testing
        _dispatcher.When(x => x.Invoke(Arg.Any<Action>()))
            .Do(x => x.Arg<Action>()());

        _sut = new ActivityLogViewModel(_activityLogger, _dispatcher);
    }

    [Fact]
    public void SetFilter_ValidLevel_UpdatesSelectedFilter()
    {
        _sut.SetFilterCommand.Execute("Warning");

        _sut.SelectedFilter.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public void SetFilter_InvalidLevel_DefaultsToTrace()
    {
        _sut.SetFilterCommand.Execute("Invalid");

        _sut.SelectedFilter.ShouldBe(LogLevel.Trace);
    }

    [Fact]
    public void IsAllSelected_WhenTraceFilter_ReturnsTrue()
    {
        _sut.SetFilterCommand.Execute("Trace");

        _sut.IsAllSelected.ShouldBeTrue();
        _sut.IsInfoSelected.ShouldBeFalse();
    }

    [Fact]
    public void IsInfoSelected_WhenInfoFilter_ReturnsTrue()
    {
        _sut.SetFilterCommand.Execute("Information");

        _sut.IsInfoSelected.ShouldBeTrue();
        _sut.IsAllSelected.ShouldBeFalse();
    }

    [Fact]
    public void IsWarningSelected_WhenWarningFilter_ReturnsTrue()
    {
        _sut.SetFilterCommand.Execute("Warning");

        _sut.IsWarningSelected.ShouldBeTrue();
    }

    [Fact]
    public void IsErrorSelected_WhenErrorFilter_ReturnsTrue()
    {
        _sut.SetFilterCommand.Execute("Error");

        _sut.IsErrorSelected.ShouldBeTrue();
    }

    [Fact]
    public void ClearLog_CallsActivityLoggerClear()
    {
        _sut.ClearLogCommand.Execute(null);

        _activityLogger.Received(1).Clear();
    }

    [Fact]
    public async Task OnNavigatedToAsync_SubscribesToEvents()
    {
        await _sut.OnNavigatedToAsync();

        _activityLogger.Received(1).EntryAdded += Arg.Any<Action<LogEntry>>();
        _activityLogger.Received(1).Cleared += Arg.Any<Action>();
    }

    [Fact]
    public async Task OnNavigatedFromAsync_UnsubscribesFromEvents()
    {
        await _sut.OnNavigatedFromAsync();

        _activityLogger.Received(1).EntryAdded -= Arg.Any<Action<LogEntry>>();
        _activityLogger.Received(1).Cleared -= Arg.Any<Action>();
    }

    [Fact]
    public async Task OnNavigatedToAsync_PopulatesFilteredEntriesFromLogger()
    {
        var entries = new List<LogEntry>
        {
            new(DateTime.Now, LogLevel.Information, "Test", "Message 1"),
            new(DateTime.Now, LogLevel.Warning, "Test", "Message 2"),
        };
        _activityLogger.Entries.Returns(entries);

        await _sut.OnNavigatedToAsync();

        _sut.FilteredEntries.Count.ShouldBe(2);
    }

    [Fact]
    public async Task OnNavigatedToAsync_WithWarningFilter_FiltersEntries()
    {
        var entries = new List<LogEntry>
        {
            new(DateTime.Now, LogLevel.Information, "Test", "Info"),
            new(DateTime.Now, LogLevel.Warning, "Test", "Warning"),
            new(DateTime.Now, LogLevel.Error, "Test", "Error"),
        };
        _activityLogger.Entries.Returns(entries);

        _sut.SetFilterCommand.Execute("Warning");
        await _sut.OnNavigatedToAsync();

        _sut.FilteredEntries.Count.ShouldBe(2);
        _sut.FilteredEntries.ShouldAllBe(e => e.Level >= LogLevel.Warning);
    }
}
