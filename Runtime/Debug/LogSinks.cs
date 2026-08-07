using System;
using System.IO;

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
            string messageFormatted = FormatMessage(entry);

            switch (entry.Level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(messageFormatted, entry.Context);
                    break;
            
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(messageFormatted, entry.Context);
                    break;

                case LogLevel.Info:
                case LogLevel.Debug:
                case LogLevel.Trace:
                default:
                    UnityEngine.Debug.Log(messageFormatted, entry.Context);
                    break;
            }
        }

        private static string FormatMessage(LogEntry entry)
        {
            string fileName = Path.GetFileName(entry.CallerFilePath);
            return $"[{entry.Level}] [{entry.Category.Name}] [{fileName}:{entry.CallerLineNumber} in {entry.CallerMemberName}()] [{entry.Timestamp}]\n{entry.Message}\n\nTrace:";
        }
    }

    /// <summary>
    /// Stub file sink. Not yet implemented — included as an extension point.
    /// </summary>
    public sealed class FileSink : ILogSink
    {
        public void Write(LogEntry entry)
        {
            // This is fine for now
            throw new NotImplementedException("FileSink is not yet implemented.");
        }
    }
}
