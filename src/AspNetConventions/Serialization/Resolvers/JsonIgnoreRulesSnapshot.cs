using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Immutable snapshot of JSON ignore rules.
    /// </summary>
    /// <param name="PropertyTypeRules">Rules for ignoring properties based on their declaring type and property name.</param>
    /// <param name="GlobalPropertyRules">Rules for ignoring properties based on their name, regardless of declaring type.</param>
    /// <param name="AssemblyRules">Rules for ignoring all properties of types within specific assemblies.</param>
    /// <param name="TypeRules">Rules for ignoring all properties of specific types.</param>
    public sealed record JsonIgnoreRulesSnapshot(
        IReadOnlyDictionary<Type, IReadOnlyDictionary<string, JsonIgnoreCondition>> PropertyTypeRules,
        IReadOnlyDictionary<string, JsonIgnoreCondition> GlobalPropertyRules,
        IReadOnlyDictionary<Assembly, JsonIgnoreCondition> AssemblyRules,
        IReadOnlyDictionary<Type, JsonIgnoreCondition> TypeRules
    );
}
