namespace AspNetConventions.Routing.Models
{
    public record struct RouteParameterContext(
        RouteModelContext RouteModelContext,
        string ParameterName)
    {
        public string DisplayName { get; init; } = RouteModelContext.IsAction
            ? $"{RouteModelContext}.{ParameterName}"
            : $"{RouteModelContext}/{{{ParameterName}}}";

        public override readonly string ToString() => DisplayName;
    }
}
