using System.IO;

namespace DeadWrongGames.ZServices.Diagnostics
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
                    UnityEngine.Debug.LogError(messageFormatted);
                    break;

                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(messageFormatted);
                    break;

                case LogLevel.Info:
                case LogLevel.Debug:
                case LogLevel.Trace:
                default:
                    UnityEngine.Debug.Log(messageFormatted);
                    break;
            }
        }

        private static string FormatMessage(LogEntry entry)
        {
            string header = $"[{entry.Level}] [{entry.Category.Name}] [{entry.Timestamp:HH:mm:ss.fff}]";

            // Omit the caller-info bracket when it's not available
            if (entry.CallerInfo != null)
            {
                string fileName = Path.GetFileName(entry.CallerInfo.FilePath);
                header += $" [{fileName}:{entry.CallerInfo.LineNumber} in {entry.CallerInfo.MemberName}()]";
            }

            return $"{header}\n{entry.Message}\n";
        }
    }

    /// <summary>
    /// Stub file sink. Not yet implemented
    /// </summary>
    public sealed class FileSink : ILogSink
    {
        public void Write(LogEntry entry)
        {
            // TODO: implement actual file writing.
            UnityEngine.Debug.LogWarning($"[FileSink not yet implemented. Falling back to console] {entry.Message}");
        }
    }
}