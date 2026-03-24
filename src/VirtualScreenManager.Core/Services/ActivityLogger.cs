using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using VirtualScreenManager.Core.Models;

namespace VirtualScreenManager.Core.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> Entries => _entries.ToArray();

    public event Action<LogEntry>? EntryAdded;
    public event Action? Cleared;

    public void Log(LogLevel level, string category, string message, string? detail = null)
    {
        var entry = new LogEntry(DateTime.Now, level, category, message, detail);
        _entries.Enqueue(entry);
        EntryAdded?.Invoke(entry);
    }

    public void Info(string category, string message, string? detail = null)
        => Log(LogLevel.Information, category, message, detail);

    public void Warning(string category, string message, string? detail = null)
        => Log(LogLevel.Warning, category, message, detail);

    public void Error(string category, string message, string? detail = null)
        => Log(LogLevel.Error, category, message, detail);

    public void Clear()
    {
        while (_entries.TryDequeue(out _)) { }
        Cleared?.Invoke();
    }
}
