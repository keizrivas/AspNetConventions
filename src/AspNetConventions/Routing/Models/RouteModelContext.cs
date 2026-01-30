using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;

namespace AspNetConventions.Routing.Models
{
    public readonly record struct RouteModelContext
    {
        internal RouteModelContext(SelectorModel selector, ActionModel action)
        {
            Action = action;
            Selector = selector;
            DisplayName = $"{action?.Controller.ControllerName}.{action?.ActionName}";
        }

        internal RouteModelContext(SelectorModel selector, PageRouteModel page)
        {
            Page = page;
            Selector = selector;
            DisplayName = Page.ViewEnginePath;
        }

        internal RouteModelContext(RouteEndpointBuilder routeEndpointBuilder)
        {
            RouteEndpointBuilder = routeEndpointBuilder;
            DisplayName = routeEndpointBuilder.DisplayName ?? "UnknowRouteEndpoint";
        }

        public string DisplayName { get; init; }

        public RouteEndpointBuilder? RouteEndpointBuilder { get; init; }

        public SelectorModel? Selector { get; init; }

        public PageRouteModel? Page { get; init; }

        public ActionModel? Action { get; init; }

        public ControllerModel? Controller => Action?.Controller;

        public bool IsPage => Page != null;

        public bool IsAction => Action != null;

        public bool IsRouteEndpoint => RouteEndpointBuilder != null;

        public override readonly string ToString() => DisplayName;

        public override readonly int GetHashCode() =>
            Selector.GetHashCode() + DisplayName.GetHashCode(StringComparison.Ordinal);
    }
}
