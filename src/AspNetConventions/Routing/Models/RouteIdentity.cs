using AspNetConventions.Core.Enums;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Represents the unique identity of a route, combining its source kind and identifier.
    /// </summary>
    /// <param name="Kind">The kind of route source.</param>
    /// <param name="Id">The unique identifier for the route within its source kind.</param>
    /// <remarks>
    /// This record struct provides a lightweight way to identify routes from different sources
    /// with their corresponding identifiers for tracking and transformation purposes.
    /// </remarks>
    public readonly record struct RouteIdentity(
        RouteSourceKind Kind,
        string Id);

}
