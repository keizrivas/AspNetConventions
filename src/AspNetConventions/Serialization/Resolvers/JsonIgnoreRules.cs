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
        /// <typeparam name="T">The type containing the property to ignore.</typeparam>
        /// <param name="propertySelector">An expression selecting the property to ignore.</param>
        /// <param name="condition">The condition under which the property should be ignored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertySelector"/> is null.</exception>
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

        /// <summary>
        /// Configures a specific property on a type to be ignored.
        /// </summary>
        /// <param name="type">The type containing the property to ignore.</param>
        /// <param name="propertyName">The name of the property to ignore.</param>
        /// <param name="condition">The condition under which the property should be ignored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null or empty.</exception>
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
        /// <param name="assembly">The assembly to ignore.</param>
        /// <param name="condition">The condition under which types in the assembly should be ignored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
        public void IgnoreAssembly(Assembly assembly, JsonIgnoreCondition condition)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            _assemblyRules[assembly] = condition;
        }

        /// <summary>
        /// Configures a property by name (applies to all types).
        /// </summary>
        /// <param name="propertyName">The name of the property to ignore globally.</param>
        /// <param name="condition">The condition under which the property should be ignored globally.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null or empty.</exception>
        public void IgnorePropertyName(string propertyName, JsonIgnoreCondition condition)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName);
            _globalPropertyRules[propertyName] = condition;
        }

        /// <summary>
        /// Configures a specific type to be ignored.
        /// </summary>
        /// <typeparam name="TType">The type to ignore.</typeparam>
        /// <param name="condition">The condition under which the type should be ignored.</param>
        public void IgnoreType<TType>(JsonIgnoreCondition condition)
        {
            _typeRules[typeof(TType)] = condition;
        }

        /// <summary>
        /// Extracts the member expression from a property selector expression.
        /// </summary>
        /// <typeparam name="T">The type containing the property.</typeparam>
        /// <param name="expr">The expression selecting the property.</param>
        /// <returns>The member expression representing the property access.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the expression does not select a property.</exception>
        private static MemberExpression ExtractMember<T>(Expression<Func<T, object?>> expr)
        {
            return (expr.Body as MemberExpression)
                ?? ((UnaryExpression)expr.Body)?.Operand as MemberExpression
                ?? throw new InvalidOperationException("Must select a property.");
        }
    }
}
