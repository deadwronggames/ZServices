using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeadWrongGames.ZServices.Debug
{
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
    /// Top-level logger configuration supplied by the game.
    /// </summary>
    public sealed class LoggerConfiguration
    {
        /// <summary>Global minimum log level applied when no category override exists.</summary>
        public LogLevel DefaultMinLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Global sinks used when no category-level sink override exists.
        /// Defaults to a single <see cref="UnityConsoleSink"/> if left null or empty.
        /// </summary>
        public IReadOnlyList<ILogSink> DefaultSinks { get; set; }

        /// <summary>
        /// Per-category overrides keyed by <see cref="LogCategory.Name"/>.
        /// Each entry may override the minimum level, the sinks, or both.
        /// </summary>
        public IReadOnlyDictionary<string, CategoryConfiguration> CategoryOverrides { get; set; }
    }

    /// <summary>
    /// Optional per-category overrides. Null fields fall back to the global configuration.
    /// </summary>
    public sealed class CategoryConfiguration
    {
        /// <summary>Overrides the global minimum level for this category. Null = use global.</summary>
        public LogLevel? MinLevel { get; set; }

        /// <summary>Overrides the global sinks for this category. Null = use global sinks.</summary>
        public IReadOnlyList<ILogSink> Sinks { get; set; }
    }

    /// <summary>
    /// Scans all loaded assemblies for exactly one <see cref="ILoggerConfigurationProvider"/>
    /// decorated with <see cref="LoggerConfigurationProviderAttribute"/>.
    /// </summary>
    internal static class LoggerConfigurationDiscovery
    {
        internal static LoggerConfiguration Discover()
        {
            var providerTypes = AppDomain.CurrentDomain
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

            var instance = (ILoggerConfigurationProvider)Activator.CreateInstance(providerTypes[0]);
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
