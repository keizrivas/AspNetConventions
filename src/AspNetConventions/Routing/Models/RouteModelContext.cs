using System;
using AspNetConventions.Core.Enums;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Provides context information about a route model for MVC actions, Razor Pages, or Minimal API endpoints.
    /// </summary>
    /// <remarks>
    /// This record struct encapsulates the context needed for route transformations, providing unified access
    /// to different endpoint types and their associated metadata.
    /// </remarks>
    public readonly record struct RouteModelContext
    {
        private RouteModelContext(RouteIdentity identity)
        {
            Identity = identity;
            DisplayName = identity.Id;
        }

        /// <summary>
        /// Gets the unique identity of the route associated with this instance.
        /// </summary>
        public RouteIdentity Identity { get; init; }

        /// <summary>
        /// Gets the display name of the route model.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the action model for MVC controllers.
        /// </summary>
        /// <value>null for Razor Pages and Minimal API endpoints.</value>
        public ActionModel? Action { get; init; }

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
        /// Gets the route endpoint builder for Minimal API endpoints.
        /// </summary>
        /// <value>null for MVC actions and Razor Pages.</value>
        public RouteEndpointBuilder? RouteEndpointBuilder { get; init; }

        /// <summary>
        /// Returns the display name of this route model context.
        /// </summary>
        /// <returns>The display name.</returns>
        public override readonly string ToString() => DisplayName;

        /// <summary>
        /// Initializes a new <see cref="RouteModelContext"/> for an MVC action.
        /// </summary>
        /// <param name="selector">The selector model containing route information.</param>
        /// <param name="action">The action model containing controller and action details.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="action"/> is null.</exception>
        public static RouteModelContext FromMvcAction(
            SelectorModel selector,
            ActionModel action)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(selector);

            var identity = new RouteIdentity(
                RouteSourceKind.MvcAction,
                $"{action.Controller.ControllerName}.{action.ActionName}");

            return new RouteModelContext(identity)
            {
                Selector = selector,
                Action = action
            };
        }

        /// <summary>
        /// Initializes a new <see cref="RouteModelContext"/> for a Razor Page.
        /// </summary>
        /// <param name="selector">The selector model containing route information.</param>
        /// <param name="page">The page route model containing page details.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="page"/> is null.</exception>
        public static RouteModelContext FromRazorPage(
            SelectorModel selector,
            PageRouteModel page)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(selector);

            var identity = new RouteIdentity(
                RouteSourceKind.RazorPage,
                page.ViewEnginePath);

            return new RouteModelContext(identity)
            {
                Selector = selector,
                Page = page
            };
        }

        /// <summary>
        /// Initializes a new <see cref="RouteModelContext"/> for a Minimal API route endpoint.
        /// </summary>
        /// <param name="builder">The route endpoint builder containing endpoint details.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        public static RouteModelContext FromMinimalApi(
            RouteEndpointBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var identity = new RouteIdentity(
                RouteSourceKind.MinimalApi,
                builder.DisplayName ?? builder.RoutePattern.RawText ?? "unknown");

            return new RouteModelContext(identity)
            {
                RouteEndpointBuilder = builder
            };
        }
    }
}
