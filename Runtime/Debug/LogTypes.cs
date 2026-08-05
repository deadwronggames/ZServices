using System;
using UnityEngine;

namespace DeadWrongGames.ZServices.Debug
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

    public readonly record struct LogCategory(string Name)
    {
        public static implicit operator LogCategory(string name) => new(name);
        public override string ToString() => Name;
    }

    public sealed class LogEntry
    {
        public LogLevel   Level            { get; }
        public LogCategory Category        { get; }
        public string     Message          { get; }
        public string     CallerFilePath   { get; }
        public string     CallerMemberName { get; }
        public int        CallerLineNumber { get; }
        public Object     Context         { get; }   // UnityEngine.Object, may be null
        public DateTime   Timestamp        { get; }

        public LogEntry(
            LogLevel    level,
            LogCategory category,
            string      message,
            string      callerFilePath,
            string      callerMemberName,
            int         callerLineNumber,
            Object      context)
        {
            Level            = level;
            Category         = category;
            Message          = message;
            CallerFilePath   = callerFilePath;
            CallerMemberName = callerMemberName;
            CallerLineNumber = callerLineNumber;
            Context          = context;
            Timestamp        = DateTime.UtcNow;
        }
    }
}
