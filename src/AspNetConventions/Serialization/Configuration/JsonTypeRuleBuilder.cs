using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AspNetConventions.Serialization.Resolvers;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Provides a fluent API for configuring JSON serialization rules for the properties of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for which to configure serialization rules.</typeparam>
    internal sealed class JsonTypeRuleBuilder<T> : IJsonTypeRuleBuilder<T>
    {
        private readonly Dictionary<string, JsonPropertyTypeRule> _typeRules;

        internal JsonTypeRuleBuilder(Dictionary<string, JsonPropertyTypeRule> typeRules)
        {
            _typeRules = typeRules;
        }

        public IJsonPropertyRuleBuilder Property<TProp>(Expression<Func<T, TProp>> selector)
        {
            ArgumentNullException.ThrowIfNull(selector);

            var propertyName = ExtractPropertyName(selector);
            return new JsonPropertyRuleBuilder(_typeRules, propertyName);
        }

        private static string ExtractPropertyName<TProp>(Expression<Func<T, TProp>> expr)
        {
            var member = (expr.Body as MemberExpression)
                ?? ((expr.Body as UnaryExpression)?.Operand as MemberExpression)
                ?? throw new InvalidOperationException(
                    $"Expression '{expr}' does not select a property.");

            return member.Member.Name;
        }
    }
}
