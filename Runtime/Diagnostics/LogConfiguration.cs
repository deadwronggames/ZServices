#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeadWrongGames.ZServices.Diagnostics;

/// <summary>
/// Top-level logger configuration supplied by the game.
/// </summary>
public sealed class LoggerConfiguration
{
    /// <summary>Global minimum log level applied when no category override exists.</summary>
    public static LogLevel DefaultMinLevel => LogLevel.Info;

    /// <summary>Global sinks used when no category-level sink override exists.</summary>
    public static IReadOnlyList<ILogSink> DefaultSinks { get; } = new List<ILogSink> { new UnityConsoleSink() }.AsReadOnly();

    public LoggerConfiguration(IReadOnlyDictionary<LogCategory, LogLevel>? minLogLevelOverridesByCategory = null, IReadOnlyDictionary<LogCategory, IReadOnlyList<ILogSink>>? sinkOverridesByCategory = null)
    {
        // Copy into new dictionaries so we never mutate the caller's input.
        Dictionary<LogCategory, LogLevel> minLevels = (minLogLevelOverridesByCategory != null) ? 
            new Dictionary<LogCategory, LogLevel>(minLogLevelOverridesByCategory) : 
            new Dictionary<LogCategory, LogLevel>();
        MinLogLevelOverridesByCategory = minLevels;

        Dictionary<LogCategory, IReadOnlyList<ILogSink>> sinks = (sinkOverridesByCategory != null) ?
            new Dictionary<LogCategory, IReadOnlyList<ILogSink>>(sinkOverridesByCategory) : 
            new Dictionary<LogCategory, IReadOnlyList<ILogSink>>();
        IReadOnlyList<ILogSink> unhandledExceptionSinks = DefaultSinks.Where(sink => sink is not UnityConsoleSink).ToList().AsReadOnly(); // Unity already logs to the console
        sinks.TryAdd(BuiltInLogCategories.UnhandledException, unhandledExceptionSinks); // Apply only if the caller hasn't already configured them.
        SinkOverridesByCategory = sinks;
    }

    /// <summary>Per-category minimum-level overrides keyed by <see cref="LogCategory"/>.</summary>
    public IReadOnlyDictionary<LogCategory, LogLevel> MinLogLevelOverridesByCategory { get; }

    /// <summary>Per-category sink overrides keyed by <see cref="LogCategory"/>.</summary>
    public IReadOnlyDictionary<LogCategory, IReadOnlyList<ILogSink>> SinkOverridesByCategory { get; }
}


/// <summary>
/// Apply this attribute to exactly one class in the game assembly that implements
/// <see cref="ILoggerConfigurationProvider"/>. The logger will discover it automatically
/// at startup via reflection. See package README for an example.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class LoggerConfigurationProviderAttribute : Attribute { }


/// <summary>
/// Implement this interface on a class decorated with
/// <see cref="LoggerConfigurationProviderAttribute"/> to supply logger configuration
/// from the game to the ZServices package without creating a compile-time dependency.
/// </summary>
public interface ILoggerConfigurationProvider
{
    LoggerConfiguration GetConfiguration();
}


/// <summary>
/// Scans all loaded assemblies for exactly one <see cref="ILoggerConfigurationProvider"/>
/// decorated with <see cref="LoggerConfigurationProviderAttribute"/>.
/// </summary>
internal static class LoggerConfigurationDiscovery
{
    internal static LoggerConfiguration Discover()
    {
        List<Type> providerTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(SafeGetTypes)
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsDefined(typeof(LoggerConfigurationProviderAttribute), inherit: false) &&
                typeof(ILoggerConfigurationProvider).IsAssignableFrom(t))
            .ToList();

        if (providerTypes.Count == 0)
        {
            UnityEngine.Debug.Log($"[{nameof(LogService)}] No {nameof(LoggerConfigurationProviderAttribute)} found. Using default configuration.");
            return new LoggerConfiguration();
        }

        if (providerTypes.Count > 1)
        {
            string names = string.Join(", ", providerTypes.Select(t => t.FullName));
            throw new InvalidOperationException($"[{nameof(LogService)}] Found more than one [{nameof(LoggerConfigurationProviderAttribute)}] provider. Exactly one is allowed. Found: {names}");
        }

        ILoggerConfigurationProvider instance = (ILoggerConfigurationProvider)Activator.CreateInstance(providerTypes[0]);
        return instance.GetConfiguration();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return whatever types did load successfully.
            return ex.Types.Where(t => t != null)!;
        }
    }
}