#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DeadWrongGames.ZServices.Diagnostics
{
    /// <summary>
    /// Lightweight, game-independent static logger for the ZServices package.
    ///
    /// Usage:
    ///   Logger.Info(MyCategories.Foo, "Something happened").Log();
    ///   Logger.Warning(MyCategories.Foo, "Watch out").LogOnce();
    ///
    /// Initialize once via <see cref="Initialize"/>; subsequent calls are no-ops.
    /// </summary>
    public static class LogService
    {
        // ### State
        private static LoggerConfiguration? s_config;
        private static readonly HashSet<string> s_loggedOnceKeys = new();
        private static readonly object s_loggedOnceKeysLock = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            s_config = null;
            lock (s_loggedOnceKeysLock) { s_loggedOnceKeys.Clear(); }
            Application.logMessageReceivedThreaded -= OnUnityLogMessageReceived;
        }

        // ### Initialization

        /// <summary>
        /// Discovers game configuration via reflection and sets up the logger.
        /// Safe to call multiple times, subsequent calls are no-ops.
        /// </summary>
        public static void Initialize()
        {
            if (s_config != null) return; // already initialized
            s_config = LoggerConfigurationDiscovery.Discover();
            Application.logMessageReceivedThreaded += OnUnityLogMessageReceived;
        }

        // ### Public API, one factory method per level.
        // Each returns a LogBuilder. Call .Log() or .LogOnce() to dispatch.
        // Overloads without specifying a category use BuiltInLogCategories.General
    
        public static LogBuilder Trace(
            LogCategory category, string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Trace, category, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Trace(
            string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Trace, BuiltInLogCategories.General, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Debug(
            LogCategory category, string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Debug, category, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Debug(
            string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Debug, BuiltInLogCategories.General, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Info(
            LogCategory category, string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Info, category, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));

        public static LogBuilder Info(
            string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Info, BuiltInLogCategories.General, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
        public static LogBuilder Warning(
            LogCategory category, string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Warning, category, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Warning(
            string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Warning, BuiltInLogCategories.General, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));

        public static LogBuilder Error(
            LogCategory category, string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Error, category, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Error(
            string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Error, BuiltInLogCategories.General, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));

        public static LogBuilder Fatal(
            LogCategory category, string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Fatal, category, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    
        public static LogBuilder Fatal(
            string message,
            [CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int? callerLineNumber = null)
            => new(LogLevel.Fatal, BuiltInLogCategories.General, message, CreateCallerInfo(callerFilePath, callerMemberName, callerLineNumber));
    

        private static CallerInfo? CreateCallerInfo(string? callerFilePath, string? callerMemberName, int? callerLineNumber)
        {
            if (callerFilePath == null || callerMemberName == null || callerLineNumber == null) return null;
            return new CallerInfo(callerFilePath, callerMemberName, (int)callerLineNumber);
        }

        // ### Dispatch (called by LogBuilder)

        internal static void Dispatch(LogBuilder builder, bool onlyOnce)
        {
            if (s_config == null)
                throw new InvalidOperationException($"{nameof(LogService)}.{nameof(Initialize)} has not been called.");

            // LogLevel filter
            if (builder.Level < ResolveMinLevel(builder.Category)) return;

            // onlyOnce guard, keyed on filename + line
            if (onlyOnce)
            {
                if (builder.CallerInfo == null) Warning(BuiltInLogCategories.General, "LogOnce called but no caller info was available. Logging unconditionally.").Log();
                else
                {
                    string key = $"{Path.GetFileName(builder.CallerInfo.FilePath)}:{builder.CallerInfo.LineNumber}";
                    lock (s_loggedOnceKeysLock) { if (!s_loggedOnceKeys.Add(key)) return; }
                }
            }

            LogEntry entry = new(builder.Level, builder.Category, builder.Message, builder.CallerInfo);
            foreach (ILogSink sink in ResolveSinks(builder.Category))
                sink.Write(entry);
        }

        // ### Unity exception hook

        /// <summary>
        /// Receives exceptions Unity catches and re-routes through its log pipeline.
        /// Parses the first user-code frame from the stack trace string to recover caller info.
        /// Bypasses level filtering and once-guards, unhandled exceptions always go through.
        /// </summary>
        private static void OnUnityLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (type != LogType.Exception) return;

            CallerInfo? callerInfo = ParseFirstStackFrame(stackTrace);
            LogEntry entry = new(LogLevel.Fatal, BuiltInLogCategories.UnhandledException, logString, callerInfo);

            foreach (ILogSink sink in ResolveSinks(BuiltInLogCategories.UnhandledException))
                sink.Write(entry);
        }

        /// <summary>
        /// Parses caller info from the first frame of a Unity stack trace string.
        /// Unity's format in editor and development builds is:
        ///   Namespace.Class.Method () (at Assets/Path/File.cs:42)
        /// Falls back to empty/zero sentinel values if parsing fails.
        /// </summary>
        private static CallerInfo? ParseFirstStackFrame(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace)) return null;
        
            // Take the first non-empty line
            ReadOnlySpan<char> span = stackTrace.AsSpan().TrimStart();
            int lineEnd = span.IndexOf('\n');
            ReadOnlySpan<char> firstLine = lineEnd >= 0 ? span[..lineEnd].TrimEnd() : span;

            // Extract method name, everything before the first ' ('
            string callerMemberName;
            int parenIndex = firstLine.IndexOf(" (");
            if (parenIndex > 0)
            {
                ReadOnlySpan<char> fullMethodName = firstLine[..parenIndex];
                int lastDot = fullMethodName.LastIndexOf('.');
                callerMemberName = (lastDot >= 0) ? 
                    fullMethodName[(lastDot + 1)..].ToString() : 
                    fullMethodName.ToString();
            }
            else return null;

            // Extract file path and line number from "(at Assets/...:42)"
            int atIndex = firstLine.IndexOf("(at ");
            if (atIndex < 0) return null;

            ReadOnlySpan<char> atPart = firstLine[(atIndex + 4)..]; // skip "(at "
            if (atPart.EndsWith(")")) atPart = atPart[..^1]; // strip trailing ")"

            int colonIndex = atPart.LastIndexOf(':');
            if (colonIndex < 0) return null;

            string callerFilePath = atPart[..colonIndex].ToString();

            ReadOnlySpan<char> lineNumberSpan = atPart[(colonIndex + 1)..];
            if (!int.TryParse(lineNumberSpan, out int callerLineNumber)) return null;
        
            return new CallerInfo(callerFilePath, callerMemberName, callerLineNumber);
        }

        // ### Resolution helpers

        private static LogLevel ResolveMinLevel(LogCategory category)
        {
            return s_config!.MinLogLevelOverridesByCategory.TryGetValue(category, out LogLevel level) ?
                level :
                LoggerConfiguration.DefaultMinLevel;
        }

        private static IEnumerable<ILogSink> ResolveSinks(LogCategory category)
        {
            return s_config!.SinkOverridesByCategory.TryGetValue(category, out IReadOnlyList<ILogSink>? sinks) ?
                sinks : 
                LoggerConfiguration.DefaultSinks;
        }
    }

    /// <summary>
    /// Fluent builder returned by <see cref="LogService"/>'s level methods.
    /// Inert until a terminal method is called, nothing is dispatched until
    /// you call <see cref="Log"/> or <see cref="LogOnce"/>.
    /// </summary>
    public readonly struct LogBuilder
    {
        internal LogLevel Level { get; }
        internal LogCategory Category { get; }
        internal string Message { get; }
        internal CallerInfo? CallerInfo { get; }

        internal LogBuilder(LogLevel level, LogCategory category, string message, CallerInfo? callerInfo)
        {
            Level = level;
            Category = category;
            Message = message;
            CallerInfo = callerInfo;
        }

        /// <summary>Dispatches this log entry.</summary>
        public void Log() => Diagnostics.LogService.Dispatch(this, onlyOnce: false);

        /// <summary>Dispatches this log entry once per call site. Subsequent calls from the same line are discarded.</summary>
        public void LogOnce() => Diagnostics.LogService.Dispatch(this, onlyOnce: true);
    }
}