using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Provides rules for ignoring properties during JSON serialization.
    /// </summary>
    public sealed class JsonIgnoreRules
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, JsonIgnoreCondition>>
            _propertyTypeRules = new();

        private readonly ConcurrentDictionary<string, JsonIgnoreCondition>
            _globalPropertyRules = new(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<Assembly, JsonIgnoreCondition>
            _assemblyRules = new();

        private readonly ConcurrentDictionary<Type, JsonIgnoreCondition>
            _typeRules = new();

        /// <summary>
        /// Creates an immutable snapshot of the current rules.
        /// </summary>
        public JsonIgnoreRulesSnapshot CreateSnapshot()
        {
            return new JsonIgnoreRulesSnapshot(
                PropertyTypeRules: _propertyTypeRules.ToDictionary(
                    k => k.Key,
                    v => (IReadOnlyDictionary<string, JsonIgnoreCondition>)v.Value.ToDictionary(
                        x => x.Key,
                        x => x.Value,
                        StringComparer.Ordinal
                    )
                ),
                GlobalPropertyRules: _globalPropertyRules.ToDictionary(
                    k => k.Key,
                    v => v.Value,
                    StringComparer.OrdinalIgnoreCase
                ),
                AssemblyRules: _assemblyRules.ToDictionary(
                    k => k.Key,
                    v => v.Value
                ),
                TypeRules: _typeRules.ToDictionary(
                    k => k.Key,
                    v => v.Value
                )
            );
        }

        /// <summary>
        /// Configures a specific property on a type to be ignored.
        /// </summary>
        public void IgnoreProperty<T>(
            Expression<Func<T, object?>> propertySelector,
            JsonIgnoreCondition condition)
        {
            ArgumentNullException.ThrowIfNull(propertySelector);

            var member = ExtractMember(propertySelector);
            var type = typeof(T);
            var propertyName = member.Member.Name;

            _propertyTypeRules.AddOrUpdate(
                type,
                _ => new ConcurrentDictionary<string, JsonIgnoreCondition>(StringComparer.Ordinal)
                {
                    [propertyName] = condition
                },
                (_, existing) =>
                {
                    existing[propertyName] = condition;
                    return existing;
                }
            );
        }

        public void IgnoreProperty(
            Type type,
            string propertyName,
            JsonIgnoreCondition condition)
        {
            // Store the rule keyed by the open generic type
            _propertyTypeRules.AddOrUpdate(
                type,
                _ => new ConcurrentDictionary<string, JsonIgnoreCondition>(StringComparer.Ordinal)
                {
                    [propertyName] = condition
                },
                (_, existing) =>
                {
                    existing[propertyName] = condition;
                    return existing;
                }
            );
        }

        /// <summary>
        /// Configures all types in an assembly to be ignored.
        /// </summary>
        public void IgnoreAssembly(Assembly assembly, JsonIgnoreCondition condition)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            _assemblyRules[assembly] = condition;
        }

        /// <summary>
        /// Configures a property by name (applies to all types).
        /// </summary>
        public void IgnorePropertyName(string propertyName, JsonIgnoreCondition condition)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName);
            _globalPropertyRules[propertyName] = condition;
        }

        /// <summary>
        /// Configures a specific type to be ignored.
        /// </summary>
        public void IgnoreType<TType>(JsonIgnoreCondition condition)
        {
            _typeRules[typeof(TType)] = condition;
        }

        private static MemberExpression ExtractMember<T>(Expression<Func<T, object?>> expr)
        {
            return (expr.Body as MemberExpression)
                ?? ((UnaryExpression)expr.Body)?.Operand as MemberExpression
                ?? throw new InvalidOperationException("Must select a property.");
        }
    }
}
