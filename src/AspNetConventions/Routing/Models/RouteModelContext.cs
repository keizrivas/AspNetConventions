using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Provides context information about a route model for MVC actions, Razor Pages, or Minimal API endpoints.
    /// </summary>
    /// <remarks>
    /// This record struct encapsulates the context needed for route transformations, providing unified access
    /// to different endpoint types (controllers, pages, and route endpoints) and their associated metadata.
    /// </remarks>
    public readonly record struct RouteModelContext
    {
        /// <summary>
        /// Initializes a new <see cref="RouteModelContext"/> for an MVC action.
        /// </summary>
        /// <param name="selector">The selector model containing route information.</param>
        /// <param name="action">The action model containing controller and action details.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="action"/> is null.</exception>
        internal RouteModelContext(SelectorModel selector, ActionModel action)
        {
            Action = action;
            Selector = selector;
            DisplayName = $"{action?.Controller.ControllerName}.{action?.ActionName}";
        }

        /// <summary>
        /// Initializes a new <see cref="RouteModelContext"/> for a Razor Page.
        /// </summary>
        /// <param name="selector">The selector model containing route information.</param>
        /// <param name="page">The page route model containing page details.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="page"/> is null.</exception>
        internal RouteModelContext(SelectorModel selector, PageRouteModel page)
        {
            Page = page;
            Selector = selector;
            DisplayName = Page.ViewEnginePath;
        }

        /// <summary>
        /// Initializes a new <see cref="RouteModelContext"/> for a Minimal API route endpoint.
        /// </summary>
        /// <param name="routeEndpointBuilder">The route endpoint builder containing endpoint details.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="routeEndpointBuilder"/> is null.</exception>
        internal RouteModelContext(RouteEndpointBuilder routeEndpointBuilder)
        {
            RouteEndpointBuilder = routeEndpointBuilder;
            DisplayName = routeEndpointBuilder.DisplayName ?? "UnknownRouteEndpoint";
        }

        /// <summary>
        /// Gets the display name of the route model.
        /// </summary>
        /// <value>
        /// For MVC actions: "Controller.Action" format
        /// For Razor Pages: The view engine path
        /// For route endpoints: The display name or "UnknownRouteEndpoint"
        /// </value>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the route endpoint builder for Minimal API endpoints.
        /// </summary>
        /// <value>null for MVC actions and Razor Pages.</value>
        public RouteEndpointBuilder? RouteEndpointBuilder { get; init; }

        /// <summary>
        /// Gets the selector model containing route information.
        /// </summary>
        /// <value>The selector model used for route configuration.</value>
        public SelectorModel? Selector { get; init; }

        /// <summary>
        /// Gets the page route model for Razor Pages.
        /// </summary>
        /// <value>null for MVC actions and Minimal API endpoints.</value>
        public PageRouteModel? Page { get; init; }

        /// <summary>
        /// Gets the action model for MVC controllers.
        /// </summary>
        /// <value>null for Razor Pages and Minimal API endpoints.</value>
        public ActionModel? Action { get; init; }

        /// <summary>
        /// Gets the controller model for MVC actions.
        /// </summary>
        /// <value>null for Razor Pages and Minimal API endpoints.</value>
        public ControllerModel? Controller => Action?.Controller;

        /// <summary>
        /// Gets a value indicating whether this context represents a Razor Page.
        /// </summary>
        /// <value>true if the context represents a Razor Page; otherwise, false.</value>
        public bool IsPage => Page != null;

        /// <summary>
        /// Gets a value indicating whether this context represents an MVC action.
        /// </summary>
        /// <value>true if the context represents an MVC action; otherwise, false.</value>
        public bool IsAction => Action != null;

        /// <summary>
        /// Gets a value indicating whether this context represents a Minimal API route endpoint.
        /// </summary>
        /// <value>true if the context represents a Minimal API endpoint; otherwise, false.</value>
        public bool IsRouteEndpoint => RouteEndpointBuilder != null;

        /// <summary>
        /// Returns the display name of this route model context.
        /// </summary>
        /// <returns>The display name.</returns>
        public override readonly string ToString() => DisplayName;

        /// <summary>
        /// Returns the hash code for this route model context.
        /// </summary>
        /// <returns>A hash code based on the selector and display name.</returns>
        public override readonly int GetHashCode() =>
            Selector.GetHashCode() + DisplayName.GetHashCode(StringComparison.Ordinal);
    }
}
