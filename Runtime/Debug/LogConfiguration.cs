#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DeadWrongGames.ZUtils;

namespace DeadWrongGames.ZServices.Debug
{
    /// <summary>
    /// Top-level logger configuration supplied by the game.
    /// </summary>
    public sealed class LoggerConfiguration // TODO does this need to be a class? could it not be e.g. a record struct (would still need to be discoverable of course)
    {
        /// <summary>Global minimum log level applied when no category override exists.</summary>
        public static LogLevel DefaultMinLevel => LogLevel.Info;

        /// <summary> Global sinks used when no category-level sink override exists. </summary>
        public static IReadOnlyList<ILogSink> DefaultSinks => new List<ILogSink> { new UnityConsoleSink() };

        public LoggerConfiguration(Dictionary<LogCategory, LogLevel>? categoryOverrideMinLogLevel = null, Dictionary<LogCategory, List<ILogSink>>? categoryOverrideSinks = null)
        {
            _categoryOverrideMinLogLevel = categoryOverrideMinLogLevel ?? new Dictionary<LogCategory, LogLevel>();
            _categoryOverrideMinLogLevel.Add(BuiltInLogCategories.UnhandledException, LogLevel.Fatal); // Does not mean the game necessarily crashes but an exception was not handled, I think Fatal is fair

            _categoryOverrideSinks = categoryOverrideSinks ?? new Dictionary<LogCategory, List<ILogSink>>();
            _categoryOverrideSinks.Add(BuiltInLogCategories.UnhandledException, new List<ILogSink> { new FileSink() }); // Unity already logs this in the console. 
        }

        /// <summary>
        /// Per-category overrides keyed by <see cref="LogCategory.Name"/>.
        /// Each entry may override the minimum level, the sinks, or both.
        /// </summary>
        public IReadOnlyDictionary<LogCategory, LogLevel> CategoryOverrideMinLogLevel => _categoryOverrideMinLogLevel;
        private readonly Dictionary<LogCategory, LogLevel> _categoryOverrideMinLogLevel;
        public IReadOnlyDictionary<LogCategory, List<ILogSink>> CategoryOverrideSinks => _categoryOverrideSinks;
        private readonly Dictionary<LogCategory, List<ILogSink>> _categoryOverrideSinks;
    }
    
    
    /// <summary>
    /// Apply this attribute to exactly one class in the game assembly that implements
    /// <see cref="ILoggerConfigurationProvider"/>. The logger will discover it automatically
    /// at startup via reflection — no explicit registration required.
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
                "providerTypes.Count == 0".Print();
                // No game-provided configuration — use defaults.
                return new LoggerConfiguration();
            }

            if (providerTypes.Count > 1)
            {
                string names = string.Join(", ", providerTypes.Select(t => t.FullName));
                throw new InvalidOperationException(
                    $"[ZServices.Logger] Found more than one [{nameof(LoggerConfigurationProviderAttribute)}] provider. " +
                    $"Exactly one is allowed. Found: {names}");
            }

            ILoggerConfigurationProvider? instance = (ILoggerConfigurationProvider)Activator.CreateInstance(providerTypes[0]);
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
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
