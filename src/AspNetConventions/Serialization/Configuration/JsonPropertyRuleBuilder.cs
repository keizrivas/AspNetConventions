using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AspNetConventions.Serialization.Resolvers;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Provides a fluent API for configuring JSON serialization rules for a specific property of a type.
    /// </summary>
    internal sealed class JsonPropertyRuleBuilder : IJsonPropertyRuleBuilder
    {
        private readonly Dictionary<string, JsonPropertyTypeRule> _typeRules;
        private readonly string _propertyName;

        internal JsonPropertyRuleBuilder(
            Dictionary<string, JsonPropertyTypeRule> typeRules,
            string propertyName)
        {
            _typeRules = typeRules;
            _propertyName = propertyName;

            if (!_typeRules.ContainsKey(propertyName))
            {
                _typeRules[propertyName] = new JsonPropertyTypeRule();
            }
        }

        public IJsonPropertyRuleBuilder Ignore(JsonIgnoreCondition condition = JsonIgnoreCondition.Always)
        {
            _typeRules[_propertyName] = _typeRules[_propertyName] with { Ignore = condition };
            return this;
        }

        public IJsonPropertyRuleBuilder Name(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            _typeRules[_propertyName] = _typeRules[_propertyName] with { Name = name };
            return this;
        }

        public IJsonPropertyRuleBuilder Order(int order)
        {
            _typeRules[_propertyName] = _typeRules[_propertyName] with { Order = order };
            return this;
        }
    }
}
