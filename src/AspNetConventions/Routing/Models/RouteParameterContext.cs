using AspNetConventions.Core.Enums;

namespace AspNetConventions.Routing.Models
{
    public record struct RouteParameterContext(
        RouteModelContext RouteModelContext,
        string ParameterName)
    {
        public string DisplayName { get; init; } = RouteModelContext.Identity.Kind switch
        {
            RouteSourceKind.MvcAction =>
                $"{RouteModelContext.DisplayName}.{ParameterName}",
            RouteSourceKind.RazorPage or RouteSourceKind.MinimalApi =>
                $"{RouteModelContext.DisplayName}/{{{ParameterName}}}",
            _ => ParameterName,
        };

        public override readonly string ToString() => DisplayName;
    }
}
