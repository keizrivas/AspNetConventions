using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Immutable snapshot of JSON ignore rules.
    /// </summary>
    public sealed record JsonIgnoreRulesSnapshot(
        IReadOnlyDictionary<Type, IReadOnlyDictionary<string, JsonIgnoreCondition>> PropertyTypeRules,
        IReadOnlyDictionary<string, JsonIgnoreCondition> GlobalPropertyRules,
        IReadOnlyDictionary<Assembly, JsonIgnoreCondition> AssemblyRules,
        IReadOnlyDictionary<Type, JsonIgnoreCondition> TypeRules
    );
}
