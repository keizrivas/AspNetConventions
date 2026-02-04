using AspNetConventions.Core.Enums;

namespace AspNetConventions.Routing.Models
{
    public readonly record struct RouteIdentity(
        RouteSourceKind Kind,
        string Id);

}
