using Microsoft.Extensions.Logging;
using Shouldly;
using VirtualScreenManager.Core.Models;
using VirtualScreenManager.Core.Services;
using Xunit;

namespace VirtualScreenManager.Core.Tests.Services;

public class ActivityLoggerTests
{
    private readonly ActivityLogger _sut = new();

    [Fact]
    public void Log_WithValidParameters_AddsEntryToEntries()
    {
        _sut.Log(LogLevel.Information, "Test", "Hello");

        _sut.Entries.Count.ShouldBe(1);
    }

    [Fact]
    public void Log_WithValidParameters_SetsCorrectLevelCategoryAndMessage()
    {
        _sut.Log(LogLevel.Warning, "MyCategory", "MyMessage", "MyDetail");

        var entry = _sut.Entries[0];
        entry.Level.ShouldBe(LogLevel.Warning);
        entry.Category.ShouldBe("MyCategory");
        entry.Message.ShouldBe("MyMessage");
        entry.Detail.ShouldBe("MyDetail");
    }

    [Fact]
    public void Log_WithValidParameters_RaisesEntryAddedEvent()
    {
        LogEntry? raised = null;
        _sut.EntryAdded += e => raised = e;

        _sut.Log(LogLevel.Information, "Test", "Hello");

        raised.ShouldNotBeNull();
        raised.Message.ShouldBe("Hello");
    }

    [Fact]
    public void Log_WithNullDetail_CreatesEntryWithNullDetail()
    {
        _sut.Log(LogLevel.Information, "Test", "Hello");

        _sut.Entries[0].Detail.ShouldBeNull();
    }

    [Fact]
    public void Info_DelegatesToLogWithInformationLevel()
    {
        _sut.Info("Cat", "Msg");

        _sut.Entries[0].Level.ShouldBe(LogLevel.Information);
    }

    [Fact]
    public void Warning_DelegatesToLogWithWarningLevel()
    {
        _sut.Warning("Cat", "Msg");

        _sut.Entries[0].Level.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public void Error_DelegatesToLogWithErrorLevel()
    {
        _sut.Error("Cat", "Msg");

        _sut.Entries[0].Level.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        _sut.Info("A", "1");
        _sut.Info("B", "2");
        _sut.Info("C", "3");

        _sut.Clear();

        _sut.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void Clear_RaisesClearedEvent()
    {
        var raised = false;
        _sut.Cleared += () => raised = true;

        _sut.Clear();

        raised.ShouldBeTrue();
    }

    [Fact]
    public void Entries_ReturnsAllLoggedEntries_InOrder()
    {
        _sut.Info("Cat", "First");
        _sut.Warning("Cat", "Second");
        _sut.Error("Cat", "Third");

        var entries = _sut.Entries;
        entries.Count.ShouldBe(3);
        entries[0].Message.ShouldBe("First");
        entries[1].Message.ShouldBe("Second");
        entries[2].Message.ShouldBe("Third");
    }

    [Fact]
    public async Task Log_CalledFromMultipleThreads_DoesNotLoseEntries()
    {
        const int threadCount = 10;
        const int entriesPerThread = 100;

        var tasks = Enumerable.Range(0, threadCount)
            .Select(t => Task.Run(() =>
            {
                for (int i = 0; i < entriesPerThread; i++)
                {
                    _sut.Info($"Thread{t}", $"Entry{i}");
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        _sut.Entries.Count.ShouldBe(threadCount * entriesPerThread);
    }
}
