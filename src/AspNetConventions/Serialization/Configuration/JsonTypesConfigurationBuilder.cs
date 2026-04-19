using System;
using System.Collections.Generic;
using System.Reflection;
using AspNetConventions.Core.Enums.Json;
using AspNetConventions.Serialization.Resolvers;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Provides a fluent API for configuring JSON serialization rules for multiple types and their properties.
    /// </summary>
    internal sealed class JsonTypesConfigurationBuilder : IJsonTypesConfigurationBuilder
    {
        // type (or open generic definition) → CLR property name → rule
        private readonly Dictionary<Type, Dictionary<string, JsonPropertyTypeRule>> _propertyRules = [];

        // JSON property name (case-insensitive) → ignore condition
        private readonly Dictionary<string, IgnoreCondition> _globalPropertyIgnoreRules
            = new(StringComparer.OrdinalIgnoreCase);

        // type → ignore condition (applies to all properties when that type is serialized)
        private readonly Dictionary<Type, IgnoreCondition> _typeIgnoreRules = [];

        public IJsonTypesConfigurationBuilder Type<T>(Action<IJsonTypeRuleBuilder<T>> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var builder = new JsonTypeRuleBuilder<T>(GetOrCreatePropertyRules(typeof(T)));
            configure(builder);
            return this;
        }

        public IJsonTypesConfigurationBuilder OpenGenericType<T>(Action<IJsonTypeRuleBuilder<T>> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var closed = typeof(T);
            if (!closed.IsGenericType)
            {
                throw new InvalidOperationException(
                    $"'{closed.Name}' is not a generic type. " +
                    $"Use {nameof(Type)}<T>() for non-generic types.");
            }

            var openGeneric = closed.GetGenericTypeDefinition();
            var builder = new JsonTypeRuleBuilder<T>(GetOrCreatePropertyRules(openGeneric));
            configure(builder);
            return this;
        }

        public IJsonTypesConfigurationBuilder IgnoreType<T>(
            IgnoreCondition condition = IgnoreCondition.Always)
        {
            _typeIgnoreRules[typeof(T)] = condition;
            return this;
        }

        public IJsonTypesConfigurationBuilder IgnorePropertyName(
            string name,
            IgnoreCondition condition = IgnoreCondition.Always)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            _globalPropertyIgnoreRules[name] = condition;
            return this;
        }

        /// <summary>
        /// Scans <paramref name="assembly"/> for all concrete, non-generic subclasses of
        /// <see cref="JsonTypeConfigurationBase"/>, instantiates each one, and applies its rules.
        /// </summary>
        internal void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsGenericTypeDefinition || !type.IsClass)
                {
                    continue;
                }

                if (!typeof(JsonTypeConfigurationBase).IsAssignableFrom(type))
                {
                    continue;
                }

                var config = (JsonTypeConfigurationBase)Activator.CreateInstance(type)!;
                config.ApplyCore(this);
            }
        }

        /// <summary>
        /// Produces an immutable snapshot of all collected rules.
        /// Called once during <c>SystemTextJsonAdapter.GetOptions()</c>.
        /// </summary>
        internal JsonTypeRulesSnapshot CreateSnapshot()
        {
            var propertyRulesSnapshot =
                new Dictionary<Type, IReadOnlyDictionary<string, JsonPropertyTypeRule>>(_propertyRules.Count);

            foreach (var (type, rules) in _propertyRules)
            {
                propertyRulesSnapshot[type] = new Dictionary<string, JsonPropertyTypeRule>(
                    rules, StringComparer.Ordinal);
            }

            return new JsonTypeRulesSnapshot(
                PropertyRules: propertyRulesSnapshot,
                GlobalPropertyIgnoreRules: new Dictionary<string, IgnoreCondition>(
                    _globalPropertyIgnoreRules, StringComparer.OrdinalIgnoreCase),
                TypeIgnoreRules: new Dictionary<Type, IgnoreCondition>(_typeIgnoreRules)
            );
        }

        private Dictionary<string, JsonPropertyTypeRule> GetOrCreatePropertyRules(Type type)
        {
            if (!_propertyRules.TryGetValue(type, out var typeRules))
            {
                typeRules = new Dictionary<string, JsonPropertyTypeRule>(StringComparer.Ordinal);
                _propertyRules[type] = typeRules;
            }

            return typeRules;
        }
    }
}
