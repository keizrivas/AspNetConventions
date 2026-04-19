using System;
using System.Collections.Generic;
using System.Reflection;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Enums;
using AspNetConventions.Core.Enums.Json;
using AspNetConventions.Core.Hooks;
using AspNetConventions.Serialization.Adapters;
using AspNetConventions.Serialization.Configuration;

namespace AspNetConventions.Configuration.Options
{
    /// <summary>
    /// Provides configuration options for JSON serialization.
    /// </summary>
    public sealed class JsonSerializationOptions : ICloneable
    {
        private IJsonSerializerAdapter? _serializerAdapter;
        private List<Assembly> _scanAssemblies = [];

        // Escape hatch for serializer-specific needs
        private readonly Dictionary<IJsonSerializerAdapter, Action<object>> _adapterConfigurations = [];

        /// <summary>
        /// Gets or sets whether json serialization is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the casing style for JSON property names.
        /// </summary>
        public CasingStyle CaseStyle { get; set; } = CasingStyle.CamelCase;

        /// <summary>
        /// Gets or sets a custom case converter.
        /// </summary>
        public ICaseConverter? CaseConverter { get; set; }

        /// <summary>
        /// Gets or sets an action to configure per-type JSON rules.
        /// </summary>
        public Action<IJsonTypesConfigurationBuilder>? ConfigureTypes { get; set; }

        /// <summary>
        /// Gets or sets the default ignore condition.
        /// </summary>
        public IgnoreCondition IgnoreCondition { get; set; } = IgnoreCondition.Never;

        /// <summary>
        /// Gets or sets whether property name comparison is case-insensitive.
        /// </summary>
        public bool CaseInsensitive { get; set; }

        /// <summary>
        /// Gets or sets whether to write indented JSON.
        /// </summary>
        public bool WriteIndented { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth for JSON objects.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Gets or sets whether to allow trailing commas in JSON.
        /// </summary>
        public bool AllowTrailingCommas { get; set; }

        /// <summary>
        /// Gets or sets how numbers are handled during serialization.
        /// </summary>
        public NumberHandling NumberHandling { get; set; } = NumberHandling.Strict;

        /// <summary>
        /// Gets or sets whether to use string enum converter for enum serialization.
        /// </summary>
        /// <value><see langword="true"/> to convert enums to strings; otherwise, <see langword="false"/> to serialize enums as numbers.</value>
        public bool UseStringEnumConverter { get; set; } = true;

        /// <summary>
        /// Gets or sets custom JSON converters.
        /// </summary>
        public IReadOnlyList<object> Converters { get; private set; } = [];

        /// <summary>
        /// Gets or sets the collection of hooks used to customize json serialization behavior.
        /// </summary>
        public JsonSerializationHooks Hooks { get; set; } = new();

        /// <summary>
        /// Scans the specified assemblies for all concrete, non-generic subclasses of
        /// <see cref="JsonTypeConfigurationBase"/> or <see cref="JsonOpenGenericTypeConfiguration{T}"/>
        /// and registers their rules automatically.
        /// </summary>
        /// <param name="assemblies">One or more assemblies to scan.</param>
        public void ScanAssemblies(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(assemblies);
            _scanAssemblies.AddRange(assemblies);
        }

        /// <summary>
        /// Returns the assemblies registered via <see cref="ScanAssemblies"/>.
        /// </summary>
        internal IReadOnlyList<Assembly> AssembliesToScan => _scanAssemblies;

        /// <summary>
        /// Configures a custom JSON serializer adapter with optional configuration options.
        /// </summary>
        /// <typeparam name="TAdapter">The type of serializer adapter to use.</typeparam>
        /// <typeparam name="TOptions">The type of configuration options for the adapter.</typeparam>
        /// <param name="configure">An optional delegate to configure the adapter options.</param>
        public void ConfigureAdapter<TAdapter, TOptions>(Action<TOptions>? configure = null)
          where TAdapter : IJsonSerializerAdapter
        {
            _serializerAdapter = (IJsonSerializerAdapter)Activator.CreateInstance(
                typeof(TAdapter),
                this,
                configure)!;
        }

        /// <summary>
        /// Gets the configured JSON serializer adapter, creating a default one if not configured.
        /// </summary>
        /// <returns>The <see cref="IJsonSerializerAdapter"/> instance.</returns>
        /// <remarks>
        /// If no adapter has been configured via <see cref="ConfigureAdapter{TAdapter, TOptions}"/>,
        /// a <see cref="Serialization.Adapters.SystemTextJsonAdapter"/> will be created by default.
        /// </remarks>
        public IJsonSerializerAdapter GetSerializerAdapter()
        {
            if (_serializerAdapter is not null)
            {
                return _serializerAdapter;
            }

            _serializerAdapter = new SystemTextJsonAdapter(this);
            return _serializerAdapter;
        }

        /// <summary>
        /// Gets the underlying serializer options from the configured adapter.
        /// </summary>
        /// <returns>The serializer options object.</returns>
        public object GetSerializerOptions()
        {
            return GetSerializerAdapter().GetOptions();
        }

        /// <summary>
        /// Creates a deep clone of <see cref="JsonSerializationOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="JsonSerializationOptions"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            var cloned = (JsonSerializationOptions)MemberwiseClone();
            cloned.Converters = [.. Converters];
            cloned.ConfigureTypes = ConfigureTypes;
            cloned._serializerAdapter = _serializerAdapter;
            cloned._scanAssemblies = [.. _scanAssemblies];
            cloned.Hooks = (JsonSerializationHooks)Hooks.Clone();

            return cloned;
        }
    }
}
