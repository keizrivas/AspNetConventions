using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AspNetConventions.Core.Enums.Json;
using AspNetConventions.Core.Hooks;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Resolves JSON type information with per-type, global, and type-level configuration rules.
    /// </summary>
    /// <param name="rules">
    /// The immutable snapshot of all rules collected at startup via
    /// <c>ConfigureTypes</c> and <c>ScanAssemblies</c>.
    /// </param>
    /// <param name="hooks">
    /// Optional hooks for extending the resolution pipeline with custom logic.
    /// </param>
    internal sealed class JsonTypeInfoResolver(
        JsonTypeRulesSnapshot rules,
        JsonSerializationHooks? hooks = null) : DefaultJsonTypeInfoResolver
    {
        private readonly JsonTypeRulesSnapshot _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        private readonly JsonSerializationHooks? _hooks = hooks;

        // Static cache: type + naming policy → (JSON name → CLR name).
        // Populated once per type and never mutated after that.
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>>
            _jsonToDeclaredName = new();

        /// <summary>
        /// Gets JSON type information for the specified type with all configured rules applied.
        /// </summary>
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var info = base.GetTypeInfo(type, options);

            // Only apply property-level rules to JSON objects.
            if (info.Kind != JsonTypeInfoKind.Object)
            {
                return info;
            }

            // ShouldSerializeType hook
            if (_hooks?.ShouldSerializeType != null && !_hooks.ShouldSerializeType(type))
            {
                return info;
            }

            // Build (and cache) reverse map: JSON name → CLR property name.
            var cacheKey = $"{type.FullName}|{options.PropertyNamingPolicy?.GetType().FullName ?? "none"}";
            var jsonToDeclaredName = _jsonToDeclaredName.GetOrAdd(
                cacheKey,
                _ => BuildJsonToDeclaredName(type, options));

            foreach (var property in info.Properties)
            {
                var clrName = jsonToDeclaredName.TryGetValue(property.Name, out var mapped)
                    ? mapped
                    : property.Name;

                // TypeIgnoreRules: highest priority, checked first.
                var typeIgnoreCondition = ResolveTypeIgnoreCondition(property.PropertyType);
                if (typeIgnoreCondition.HasValue)
                {
                    var propType = property.PropertyType;
                    var cond = typeIgnoreCondition.Value;
                    property.ShouldSerialize = (_, value) =>
                        ShouldSerializeProperty(value, propType, cond);
                    continue;
                }

                // Per-type per-property rule.
                var typeRule = ResolvePropertyRule(type, clrName);

                if (typeRule is not null)
                {
                    if (typeRule.Name is not null)
                    {
                        property.Name = typeRule.Name;
                    }

                    if (typeRule.Order.HasValue)
                    {
                        property.Order = typeRule.Order.Value;
                    }
                }

                // ResolvePropertyName hook
                if (_hooks?.ResolvePropertyName != null)
                {
                    var overrideName = _hooks.ResolvePropertyName(clrName, type);
                    if (overrideName is not null)
                    {
                        property.Name = overrideName;
                    }
                }

                // Ignore condition.
                var condition = typeRule?.Ignore
                    ?? ResolveGlobalIgnoreCondition(clrName, property.Name);

                // ShouldSerializeProperty hook
                if (_hooks?.ShouldSerializeProperty != null || condition.HasValue)
                {
                    var propertyType = property.PropertyType;
                    var capturedClrName = clrName;
                    var capturedCondition = condition;
                    var capturedType = type;

                    property.ShouldSerialize = (instance, value) =>
                    {
                        // Ignore condition takes priority over the global hook.
                        if (capturedCondition.HasValue &&
                            !ShouldSerializeProperty(value, propertyType, capturedCondition.Value))
                        {
                            return false;
                        }

                        return _hooks?.ShouldSerializeProperty == null
                            || _hooks.ShouldSerializeProperty(instance, value, capturedClrName, capturedType);
                    };
                }
            }

            // OnTypeResolved hook
            if (_hooks?.OnTypeResolved != null)
            {
                var names = new List<string>(info.Properties.Count);
                foreach (var p in info.Properties)
                {
                    names.Add(p.Name);
                }
                _hooks.OnTypeResolved(type, names);
            }

            return info;
        }

        /// <summary>
        /// Walks the inheritance chain (and interfaces) of <paramref name="type"/> looking for
        /// a per-property rule registered for <paramref name="clrPropertyName"/>.
        /// </summary>
        private JsonPropertyTypeRule? ResolvePropertyRule(Type type, string clrPropertyName)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                var key = current.IsGenericType ? current.GetGenericTypeDefinition() : current;

                if (_rules.PropertyRules.TryGetValue(key, out var props) &&
                    props.TryGetValue(clrPropertyName, out var rule))
                {
                    return rule;
                }
            }

            foreach (var iface in type.GetInterfaces())
            {
                var key = iface.IsGenericType ? iface.GetGenericTypeDefinition() : iface;

                if (_rules.PropertyRules.TryGetValue(key, out var props) &&
                    props.TryGetValue(clrPropertyName, out var rule))
                {
                    return rule;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks <c>TypeIgnoreRules</c> by walking the VALUE TYPE's inheritance chain.
        /// Returns on the first match.
        /// </summary>
        private IgnoreCondition? ResolveTypeIgnoreCondition(Type propertyValueType)
        {
            for (var current = propertyValueType; current != null; current = current.BaseType)
            {
                var key = current.IsGenericType ? current.GetGenericTypeDefinition() : current;

                if (_rules.TypeIgnoreRules.TryGetValue(key, out var rule))
                {
                    return rule;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks <c>GlobalPropertyIgnoreRules</c>.
        /// Tries the CLR name first (e.g. <c>StatusCode</c>) so callers can use C# names,
        /// then falls back to the JSON name (e.g. <c>status_code</c>).
        /// </summary>
        private IgnoreCondition? ResolveGlobalIgnoreCondition(
            string clrPropertyName,
            string jsonPropertyName)
        {
            if (_rules.GlobalPropertyIgnoreRules.TryGetValue(clrPropertyName, out var rule) ||
                _rules.GlobalPropertyIgnoreRules.TryGetValue(jsonPropertyName, out rule))
            {
                return rule;
            }

            return null;
        }

        /// <summary>
        /// Builds a reverse mapping from JSON property names to CLR property names for
        /// <paramref name="type"/>, respecting <see cref="JsonPropertyNameAttribute"/> and
        /// the active naming policy.
        /// </summary>
        private static Dictionary<string, string> BuildJsonToDeclaredName(
            Type type,
            JsonSerializerOptions options)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string jsonName;

                if (attr is not null)
                {
                    jsonName = attr.Name;
                }
                else if (options.PropertyNamingPolicy is not null)
                {
                    jsonName = options.PropertyNamingPolicy.ConvertName(prop.Name);
                }
                else
                {
                    jsonName = prop.Name;
                }

                map[jsonName] = prop.Name;
            }

            return map;
        }

        /// <summary>
        /// Determines whether a property should be serialized based on the ignore condition.
        /// </summary>
        /// <param name="value">The property value to check.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="condition">The ignore condition to apply.</param>
        /// <returns>true if the property should be serialized; otherwise, false.</returns>
        private static bool ShouldSerializeProperty(
            object? value,
            Type propertyType,
            IgnoreCondition condition)
        {
            return condition switch
            {
                IgnoreCondition.Never => true,
                IgnoreCondition.WhenWritingDefault => !IsDefaultValue(value, propertyType),
                IgnoreCondition.WhenWritingNull => value is not null,
                IgnoreCondition.Always => false,
                _ => true
            };
        }

        /// <summary>
        /// Determines whether a value is the default value for its type.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <returns>true if the value is the default for its type; otherwise, false.</returns>
        private static bool IsDefaultValue(object? value, Type propertyType)
        {
            if (value is null)
            {
                return !propertyType.IsValueType ||
                       Nullable.GetUnderlyingType(propertyType) != null;
            }

            if (!propertyType.IsValueType)
            {
                return false;
            }

            return value.Equals(Activator.CreateInstance(propertyType));
        }
    }
}
