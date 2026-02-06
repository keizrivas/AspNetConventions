using AspNetConventions.Core.Enums;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Provides context information for route parameter transformation decisions.
    /// </summary>
    /// <param name="RouteModelContext">The route model context containing information about the parent route.</param>
    /// <param name="ParameterName">The name of the route parameter being evaluated.</param>
    /// <remarks>
    /// This record struct encapsulates the information needed to make transformation decisions
    /// for route parameters, including the route model context and the parameter name.
    /// </remarks>
    public record struct RouteParameterContext(
        RouteModelContext RouteModelContext,
        string ParameterName)
    {
        /// <summary>
        /// Gets a formatted display name for the parameter context.
        /// </summary>
        /// <value>The source display name.</value>
        public string DisplayName { get; init; } = RouteModelContext.Identity.Kind switch
        {
            RouteSourceKind.MvcAction =>
                $"{RouteModelContext.DisplayName}.{ParameterName}",
            RouteSourceKind.RazorPage or RouteSourceKind.MinimalApi =>
                $"{RouteModelContext.DisplayName}/{{{ParameterName}}}",
            _ => ParameterName,
        };

        /// <summary>
        /// Returns the display name of this route parameter context.
        /// </summary>
        /// <returns>The formatted display name.</returns>
        public override readonly string ToString() => DisplayName;
    }
}
