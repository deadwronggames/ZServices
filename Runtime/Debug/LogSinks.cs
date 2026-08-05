using System;
using System.IO;
using UnityEngine;

namespace DeadWrongGames.ZServices.Debug
{
    public interface ILogSink
    {
        void Write(LogEntry entry);
    }

    /// <summary>
    /// Default sink. Routes log entries to the Unity console using the appropriate
    /// Unity log method based on <see cref="LogLevel"/>.
    /// </summary>
    public sealed class UnityConsoleSink : ILogSink
    {
        public void Write(LogEntry entry)
        {
            string formatted = Format(entry);

            switch (entry.Level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formatted, entry.Context);
                    break;

                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formatted, entry.Context);
                    break;

                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(formatted, entry.Context);
                    break;

                default:
                    UnityEngine.Debug.Log(formatted, entry.Context);
                    break;
            }
        }

        private static string Format(LogEntry entry)
        {
            string fileName = Path.GetFileName(entry.CallerFilePath);
            return $"[{entry.Level}] [{entry.Category}] {entry.Message}  ({fileName}:{entry.CallerLineNumber} in {entry.CallerMemberName})";
        }
    }

    /// <summary>
    /// Stub file sink. Not yet implemented — included as an extension point.
    /// </summary>
    public sealed class FileSink : ILogSink
    {
        public void Write(LogEntry entry)
        {
            throw new NotImplementedException("FileSink is not yet implemented.");
        }
    }
}
