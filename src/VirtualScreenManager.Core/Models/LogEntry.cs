using Microsoft.Extensions.Logging;

namespace VirtualScreenManager.Core.Models;

public record LogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    string? Detail = null);
