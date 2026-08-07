#nullable enable

using System;

namespace DeadWrongGames.ZServices.Debug;

public enum LogLevel
{
    Trace   = 0,
    Debug   = 1,
    Info    = 2,
    Warning = 3,
    Error   = 4,
    Fatal   = 5,
}

public readonly record struct LogCategory(string Name);

public static class BuiltInLogCategories
{
    public static readonly LogCategory General = new("General");
    public static readonly LogCategory UnhandledException = new("UnhandledException");
}

public sealed record LogEntry(
    LogLevel Level,
    LogCategory Category,
    string Message,
    string CallerFilePath,
    string CallerMemberName,
    int CallerLineNumber,
    UnityEngine.Object? Context)
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
