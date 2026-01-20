using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

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

        public string DisplayName { get; init; }

        public SelectorModel Selector { get; init; }

        public PageRouteModel? Page { get; init; }

        public ActionModel? Action { get; init; }

        public ControllerModel? Controller => Action?.Controller;

        public bool IsPage => Page != null;

        public bool IsAction => Action != null;

        public override readonly string ToString() => DisplayName;

        public override readonly int GetHashCode() =>
            Selector.GetHashCode() + DisplayName.GetHashCode(StringComparison.Ordinal);
    }
}
