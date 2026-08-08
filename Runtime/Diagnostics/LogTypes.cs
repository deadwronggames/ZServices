#nullable enable
using System;
namespace DeadWrongGames.ZServices.Diagnostics
{
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
        CallerInfo? CallerInfo)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Caller location captured at the log call site.
    /// Null when not available (e.g. synthetic entries from unhandled exceptions that could not be parsed).
    /// </summary>
    public sealed record CallerInfo(
        string FilePath,
        string MemberName,
        int LineNumber);
}