using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace DeadWrongGames.ZServices.Debug
{
    /// <summary>
    /// Lightweight, game-independent static logger for the ZServices package.
    ///
    /// Usage:
    ///   Logger.Log(LogLevel.Info, "MyCategory", "Something happened");
    ///   Logger.Log(LogLevel.Warning, "MyCategory", "Watch out", onlyOnce: true, context: gameObject);
    ///
    /// Initialize once via <see cref="Initialize"/>; subsequent calls are no-ops.
    /// </summary>
    public static class Logger
    {
        // ── State ─────────────────────────────────────────────────────────────

        private static volatile bool                                   s_isInitialized;
        private static LoggerConfiguration                             s_config;
        private static IReadOnlyList<ILogSink>                         s_defaultSinks;

        // Guarded by _onlyOnceLock
        private static readonly HashSet<(string file, int line)>       s_onlyOnceSeen = new();
        private static readonly object                                 s_onlyOnceLock = new();

        // ── Initialization ────────────────────────────────────────────────────

        /// <summary>
        /// Discovers game configuration via reflection and sets up the logger.
        /// Safe to call multiple times — only the first call has any effect.
        /// </summary>
        public static void Initialize()
        {
            // Fast path, no lock needed for the read because s_isInitialized is volatile.
            if (s_isInitialized) return;

            // Discover and apply configuration.
            s_config       = LoggerConfigurationDiscovery.Discover();
            s_defaultSinks = ResolveDefaultSinks(s_config);

            // Subscribe to Unity's log message pipeline to capture unhandled exceptions.
            Application.logMessageReceivedThreaded += OnUnityLogMessageReceived;

            s_isInitialized = true;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Logs a message at the specified level under the given category.
        /// </summary>
        /// <param name="level">Severity of the message.</param>
        /// <param name="category">Logical grouping for this message (e.g. "AI", "Physics").</param>
        /// <param name="message">Human-readable log message.</param>
        /// <param name="context">Optional Unity Object to highlight in the hierarchy when clicked.</param>
        /// <param name="onlyOnce">When true, this message is suppressed after its first emission (keyed on caller file + line).</param>
        /// <param name="callerFilePath">Filled automatically by the compiler, do not pass manually.</param>
        /// <param name="callerMemberName">Filled automatically by the compiler, do not pass manually.</param>
        /// <param name="callerLineNumber">Filled automatically by the compiler, do not pass manually.</param>
        public static void Log(
            LogLevel    level,
            LogCategory category,
            string      message,
            Object      context          = null,
            bool        onlyOnce         = false,
            [CallerFilePath]   string callerFilePath   = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int    callerLineNumber  = 0)
        {
            if (!s_isInitialized) return; // TODO maybe throw something

            // ── onlyOnce guard ────────────────────────────────────────────────
            if (onlyOnce)
            {
                string fileName = Path.GetFileName(callerFilePath);
                (string fileName, int callerLineNumber) key = (fileName, callerLineNumber);

                lock (s_onlyOnceLock)
                {
                    if (!s_onlyOnceSeen.Add(key)) return; // already seen — discard
                }
            }

            // ── Level filter ──────────────────────────────────────────────────
            LogLevel minLevel = ResolveMinLevel(category);
            if (level < minLevel) return;

            // ── Build entry and route ─────────────────────────────────────────
            var entry = new LogEntry(level, category, message, callerFilePath, callerMemberName, callerLineNumber, context);
            var sinks = ResolveSinks(category);

            foreach (var sink in sinks)
            {
                sink.Write(entry);
            }
        }

        // ── Unity exception hook ───────────────────────────────────────────────

        /// <summary>
        /// Receives all messages Unity routes through its own logging pipeline.
        /// We only forward exceptions that were not caught anywhere else.
        /// </summary>
        private static void OnUnityLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (type != LogType.Exception) return;

            // Build a synthetic entry — no caller info available from Unity's pipeline.
            var entry = new LogEntry(
                level:            LogLevel.Fatal,
                category:         "UnhandledException",
                message:          $"{logString}\n{stackTrace}",
                callerFilePath:   "",
                callerMemberName: "",
                callerLineNumber: 0,
                context:          null);

            var sinks = ResolveSinks(entry.Category);
            foreach (var sink in sinks)
            {
                // Avoid re-entering the Unity console sink — Unity already printed this.
                // Forward to any non-Unity sinks (e.g. FileSink) only.
                if (sink is UnityConsoleSink) continue;
                sink.Write(entry);
            }
        }

        // ── Resolution helpers ────────────────────────────────────────────────

        private static LogLevel ResolveMinLevel(LogCategory category)
        {
            if (s_config.CategoryOverrides != null &&
                s_config.CategoryOverrides.TryGetValue(category.Name, out var cat) &&
                cat.MinLevel.HasValue)
            {
                return cat.MinLevel.Value;
            }

            return s_config.DefaultMinLevel;
        }

        private static IReadOnlyList<ILogSink> ResolveSinks(LogCategory category)
        {
            if (s_config.CategoryOverrides != null &&
                s_config.CategoryOverrides.TryGetValue(category.Name, out var cat) &&
                cat.Sinks != null)
            {
                return cat.Sinks;
            }

            return s_defaultSinks;
        }

        private static IReadOnlyList<ILogSink> ResolveDefaultSinks(LoggerConfiguration config)
        {
            if (config.DefaultSinks != null && config.DefaultSinks.Count > 0)
                return config.DefaultSinks;

            return new ILogSink[] { new UnityConsoleSink() };
        }
    }
}
