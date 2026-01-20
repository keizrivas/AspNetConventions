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
    internal sealed class JsonTypeInfoResolver(JsonIgnoreRulesSnapshot rules) : DefaultJsonTypeInfoResolver
    {
        private readonly JsonIgnoreRulesSnapshot _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>>
            _jsonToDeclaredName = new();

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var info = base.GetTypeInfo(type, options);

            if (info.Kind != JsonTypeInfoKind.Object)
            {
                return info;
            }

            // Build reverse map: JSON name -> C# property name
            var cacheKey = $"{type.FullName}|{options.PropertyNamingPolicy?.GetType().Name ?? "none"}";
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

        private JsonIgnoreCondition? ResolveCondition(
            Type type,
            string originalPropertyName,
            string jsonPropertyName)
        {
            // 1. Type-specific property rules (case-sensitive)
            if (_rules.PropertyTypeRules.TryGetValue(type, out var properties) &&
                properties.TryGetValue(originalPropertyName, out var propertyRule))
            {
                return propertyRule;
            }

            // 2. Global property rules (case-insensitive)
            if (_rules.GlobalPropertyRules.TryGetValue(jsonPropertyName, out var globalRule))
            {
                return globalRule;
            }

            // 3. Assembly rules
            if (type.Assembly != null &&
                _rules.AssemblyRules.TryGetValue(type.Assembly, out var assemblyRule))
            {
                return assemblyRule;
            }

            // 4. Interface rules
            foreach (var iType in type.GetInterfaces())
            {
                if (_rules.TypeRules.TryGetValue(iType, out var interfaceRule))
                {
                    return interfaceRule;
                }
            }

            // 5. Type rules
            if (_rules.TypeRules.TryGetValue(type, out var typeRule))
            {
                return typeRule;
            }

            return null;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDefaultValue(object? value, Type propertyType)
        {
            if (value is null)
            {
                return !propertyType.IsValueType ||
                       Nullable.GetUnderlyingType(propertyType) != null;
            }

            if (!propertyType.IsValueType)
                return false;

            return value.Equals(Activator.CreateInstance(propertyType));
        }
    }
}
