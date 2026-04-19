using System;
using System.Collections.Generic;
using AspNetConventions.Core.Enums.Json;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Immutable snapshot of all JSON type configuration rules collected at startup.
    /// </summary>
    /// <param name="PropertyRules">
    /// Per-type per-property rules keyed by declaring type (or open generic definition)
    /// then by CLR property name.
    /// </param>
    /// <param name="GlobalPropertyIgnoreRules">
    /// Property-name ignore rules that apply across all types, keyed by property name
    /// (case-insensitive). Lower priority than <paramref name="PropertyRules"/>.
    /// </param>
    /// <param name="TypeIgnoreRules">
    /// Type-level ignore rules: when a type is serialized, the specified
    /// condition is applied to all of its properties.
    /// </param>
    internal sealed record JsonTypeRulesSnapshot(
        IReadOnlyDictionary<Type, IReadOnlyDictionary<string, JsonPropertyTypeRule>> PropertyRules,
        IReadOnlyDictionary<string, IgnoreCondition> GlobalPropertyIgnoreRules,
        IReadOnlyDictionary<Type, IgnoreCondition> TypeIgnoreRules
    );
}
