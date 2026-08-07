#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DeadWrongGames.ZServices.Debug;

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
    // ### State
    private static LoggerConfiguration? s_config;
    private static readonly HashSet<(string file, int line)> s_loggedOnceKeys = new(); // Guarded by lock below
    private static readonly object s_loggedOnceKeysLock = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void UnsubscribeFromUnityLogMessages()
    {
        // Avoid duplicate logs e.g. in Unity Editor
        Application.logMessageReceivedThreaded -= OnUnityLogMessageReceived;
    }
    
    // ### Initialization
    /// <summary>
    /// Discovers game configuration via reflection and sets up the logger.
    /// </summary>
    public static void Initialize()
    {
        s_config = LoggerConfigurationDiscovery.Discover();
        s_loggedOnceKeys.Clear();

        // Subscribe to Unity's log message pipeline to capture unhandled exceptions.
        Application.logMessageReceivedThreaded += OnUnityLogMessageReceived;
    }

    // ### Public API

    /// <summary>
    /// Logs a message at the specified level under the given category.
    /// </summary>
    /// <param name="level">Severity of the message.</param>
    /// <param name="logCategory">Logical grouping for this message (e.g. "AI", "Physics").</param>
    /// <param name="message">Human-readable log message.</param>
    /// <param name="context">Optional Unity Object to highlight in the hierarchy when clicked.</param>
    /// <param name="onlyOnce">When true, this message is suppressed after its first emission (keyed on caller file + line).</param>
    /// <param name="callerFilePath">Filled automatically by the compiler, do not pass manually.</param>
    /// <param name="callerMemberName">Filled automatically by the compiler, do not pass manually.</param>
    /// <param name="callerLineNumber">Filled automatically by the compiler, do not pass manually.</param>
    public static void Log(
        LogLevel level,
        LogCategory logCategory,
        string message,
        UnityEngine.Object? context = null,
        bool onlyOnce = false,
        [CallerFilePath] string callerFilePath   = "",
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber  = 0)
    {
        if (s_config == null)
            throw new InvalidOperationException($"{nameof(Logger)}.{nameof(Log)} has not been called.");

        // LogLevel filter
        LogLevel minLevel = ResolveMinLevel(logCategory);
        if (level < minLevel) return;
        
        // onlyOnce guard
        if (onlyOnce)
        {
            string fileName = Path.GetFileName(callerFilePath);
            (string fileName, int callerLineNumber) key = (fileName, callerLineNumber);

            lock (s_loggedOnceKeysLock)
            {
                if (!s_loggedOnceKeys.Add(key)) return; // already seen — discard
            }
        }
        
        // Build entry and route
        LogEntry entry = new(level, logCategory, message, callerFilePath, callerMemberName, callerLineNumber, context);
        IEnumerable<ILogSink> sinks = ResolveSinks(logCategory);

        foreach (ILogSink sink in sinks)
            sink.Write(entry);
    }

    // ### Unity exception hook

    /// <summary>
    /// Receives all messages Unity routes through its own logging pipeline.
    /// We only forward exceptions that were not caught anywhere else.
    /// </summary>
    private static void OnUnityLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (type != LogType.Exception) return;
        // Build a synthetic entry — no caller info available from Unity's pipeline.
        LogEntry entry = new(
            Level:            LogLevel.Fatal,
            Category:         BuiltInLogCategories.UnhandledException,
            Message:          $"{logString}\n{stackTrace}",
            CallerFilePath:   "",
            CallerMemberName: "",
            CallerLineNumber: 0,
            Context:          null);

        IEnumerable<ILogSink> sinks = ResolveSinks(entry.Category);
        foreach (ILogSink sink in sinks)
        {
            sink.Write(entry);
        }
    }

    // ### Resolution helpers

    private static LogLevel ResolveMinLevel(LogCategory category)
    {
        return s_config!.CategoryOverrideMinLogLevel.TryGetValue(category, out LogLevel minimumLevel) ? minimumLevel : LoggerConfiguration.DefaultMinLevel;
    }

    private static IEnumerable<ILogSink> ResolveSinks(LogCategory category)
    {
        return (s_config!.CategoryOverrideSinks.TryGetValue(category, out List<ILogSink> sinks)) ? sinks : LoggerConfiguration.DefaultSinks;
    }
}
