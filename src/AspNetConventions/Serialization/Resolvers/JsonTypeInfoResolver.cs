using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Resolves JSON type information with custom ignore rules.
    /// </summary>
    /// <param name="rules">The snapshot of JSON ignore rules to apply during type information resolution.</param>
    internal sealed class JsonTypeInfoResolver(JsonIgnoreRulesSnapshot rules) : DefaultJsonTypeInfoResolver
    {
        private readonly JsonIgnoreRulesSnapshot _rules = rules ?? throw new ArgumentNullException(nameof(rules));

        private static readonly ConcurrentDictionary<string, Dictionary<string, string>>
            _jsonToDeclaredName = new();

        /// <summary>
        /// Gets JSON type information for the specified type with custom ignore rules applied.
        /// </summary>
        /// <param name="type">The type to get JSON type information for.</param>
        /// <param name="options">The JSON serializer options.</param>
        /// <returns>The JSON type information with custom ignore rules applied.</returns>
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var info = base.GetTypeInfo(type, options);

            if (info.Kind != JsonTypeInfoKind.Object)
            {
                return info;
            }
            
            // Build reverse map: JSON name -> CLR property name
            var cacheKey = $"{type.FullName}|{options.PropertyNamingPolicy?.GetType().FullName ?? "none"}";
            var jsonToDeclaredName = _jsonToDeclaredName.GetOrAdd(
                cacheKey,
                _ => BuildJsonToDeclaredName(type, options));

            // Apply ignore rules
            foreach (var property in info.Properties)
            {
                var originalPropertyName = jsonToDeclaredName.TryGetValue(property.Name, out var csharpName)
                    ? csharpName
                    : property.Name;

                var condition = ResolveCondition(type, originalPropertyName, property.Name);
                if (condition.HasValue)
                {
                    var propertyType = property.PropertyType;
                    property.ShouldSerialize = (obj, value) =>
                        ShouldSerializeProperty(value, propertyType, condition.Value);
                }
            }

            return info;
        }

        /// <summary>
        /// Builds a reverse mapping from JSON property names to CLR property names.
        /// </summary>
        /// <param name="type">The type to analyze.</param>
        /// <param name="options">The JSON serializer options containing naming policy.</param>
        /// <returns>A dictionary mapping JSON property names to their original CLR property names.</returns>
        private static Dictionary<string, string> BuildJsonToDeclaredName(
            Type type,
            JsonSerializerOptions options)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            var properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string jsonName;

                if (attr != null)
                {
                    jsonName = attr.Name;
                }
                else if (options.PropertyNamingPolicy != null)
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
        /// Resolves the JSON ignore condition for a property based on configured rules.
        /// </summary>
        /// <param name="type">The type containing the property.</param>
        /// <param name="originalPropertyName">The original CLR property name.</param>
        /// <param name="jsonPropertyName">The JSON property name after naming policy transformation.</param>
        /// <returns>The ignore condition to apply, or null if no rule matches.</returns>
        private JsonIgnoreCondition? ResolveCondition(
        Type type,
        string originalPropertyName,
        string jsonPropertyName)
        {
            // 1. Walk inheritance chain (most specific → least specific)
            for (var current = type; current != null; current = current.BaseType)
            {
                var lookupType = current.IsGenericType
                    ? current.GetGenericTypeDefinition()
                    : current;

                if (_rules.PropertyTypeRules.TryGetValue(lookupType, out var props) &&
                    props.TryGetValue(originalPropertyName, out var rule))
                {
                    return rule;
                }
            }

            // 2. Interfaces
            foreach (var iface in type.GetInterfaces())
            {
                var ifaceType = iface.IsGenericType
                    ? iface.GetGenericTypeDefinition()
                    : iface;

                if (_rules.PropertyTypeRules.TryGetValue(ifaceType, out var props) &&
                    props.TryGetValue(originalPropertyName, out var rule))
                {
                    return rule;
                }
            }

            // 3. Global property rules
            if (_rules.GlobalPropertyRules.TryGetValue(jsonPropertyName, out var globalRule))
            {
                return globalRule;
            }

            // 4. Assembly rule
            if (_rules.AssemblyRules.TryGetValue(type.Assembly, out var assemblyRule))
            {
                return assemblyRule;
            }

            // 5. Type rules (walk base types again)
            for (var current = type; current != null; current = current.BaseType)
            {
                var lookupType = current.IsGenericType
                    ? current.GetGenericTypeDefinition()
                    : current;

                if (_rules.TypeRules.TryGetValue(lookupType, out var typeRule))
                {
                    return typeRule;
                }
            }

            return null;
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
            JsonIgnoreCondition condition)
        {
            return condition switch
            {
                JsonIgnoreCondition.Never => true,
                JsonIgnoreCondition.WhenWritingDefault => !IsDefaultValue(value, propertyType),
                JsonIgnoreCondition.WhenWritingNull => value is not null,
                JsonIgnoreCondition.Always => false,
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
