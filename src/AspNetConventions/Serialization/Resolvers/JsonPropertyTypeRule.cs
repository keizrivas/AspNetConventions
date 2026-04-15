using System.Text.Json.Serialization;

namespace AspNetConventions.Serialization.Resolvers
{
    /// <summary>
    /// Represents JSON serialization rules for a specific property of a type, such as its JSON name, serialization order, and ignore condition.
    /// </summary>
    /// <param name="Ignore">The ignore condition for the property.</param>
    /// <param name="Name">The JSON name for the property.</param>
    /// <param name="Order">The serialization order for the property.</param>
    internal sealed record JsonPropertyTypeRule(
        JsonIgnoreCondition? Ignore = null,
        string? Name = null,
        int? Order = null
    );
}
