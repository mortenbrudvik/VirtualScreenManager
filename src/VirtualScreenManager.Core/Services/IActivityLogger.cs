using Microsoft.Extensions.Logging;
using VirtualScreenManager.Core.Models;

namespace VirtualScreenManager.Core.Services;

public interface IActivityLogger
{
    IReadOnlyList<LogEntry> Entries { get; }

    void Log(LogLevel level, string category, string message, string? detail = null);
    void Info(string category, string message, string? detail = null);
    void Warning(string category, string message, string? detail = null);
    void Error(string category, string message, string? detail = null);
    void Clear();

    event Action<LogEntry>? EntryAdded;
    event Action? Cleared;
}
