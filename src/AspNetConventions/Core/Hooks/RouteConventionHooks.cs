using System;
using AspNetConventions.Routing.Models;

namespace AspNetConventions.Core.Hooks
{
    /// <summary>
    /// Provides hooks for customizing route convention behavior during route transformation.
    /// </summary>
    /// <remarks>
    /// This class provides fine-grained control over route transformation by providing callbacks
    /// at various stages of the transformation process.
    /// </remarks>
    public class RouteConventionHooks : ICloneable
    {
        /// <summary>
        /// Represents a callback method to determine whether a route should be transformed.
        /// </summary>
        /// <param name="template">The route template to evaluate.</param>
        /// <param name="model">The route model context containing route information.</param>
        /// <returns>true if the route should be transformed; otherwise, false.</returns>
        public delegate bool ShouldTransformRouteCallback(string template, RouteModelContext model);

        /// <summary>
        /// Represents a callback method to determine whether a route parameter should be transformed.
        /// </summary>
        /// <param name="model">The route parameter context containing parameter information.</param>
        /// <returns>true if the parameter should be transformed; otherwise, false.</returns>
        public delegate bool ShouldTransformParameterCallback(RouteParameterContext model);

        /// <summary>
        /// Represents a callback method to determine whether a route token should be transformed.
        /// </summary>
        /// <param name="token">The route token to evaluate.</param>
        /// <returns>true if the token should be transformed; otherwise, false.</returns>
        public delegate bool ShouldTransformTokenCallback(string token);

        /// <summary>
        /// Represents a callback method called before route transformation.
        /// </summary>
        /// <param name="route">The original route template.</param>
        /// <param name="model">The route model context containing route information.</param>
        public delegate void BeforeRouteTransformCallback(string route, RouteModelContext model);

        /// <summary>
        /// Represents a callback method called after route transformation.
        /// </summary>
        /// <param name="route">The transformed route template.</param>
        /// <param name="originalRoute">The original route template before transformation.</param>
        /// <param name="model">The route model context containing route information.</param>
        public delegate void AfterRouteTransformCallback(string route, string originalRoute, RouteModelContext model);

        /// <summary>
        /// Gets or sets the callback to determine whether a route should be transformed.
        /// </summary>
        /// <value>A callback that returns false to skip transformation of the specified route.</value>
        public ShouldTransformRouteCallback? ShouldTransformRoute { get; set; }

        /// <summary>
        /// Gets or sets the callback to determine whether a route parameter should be transformed.
        /// </summary>
        /// <value>A callback that returns false to skip transformation of the specified parameter.</value>
        public ShouldTransformParameterCallback? ShouldTransformParameter { get; set; }

        /// <summary>
        /// Gets or sets the callback to determine whether a route token should be transformed.
        /// </summary>
        /// <value>A callback that returns false to skip transformation of the specified token.</value>
        public ShouldTransformTokenCallback? ShouldTransformToken { get; set; }

        /// <summary>
        /// Gets or sets the callback called before route transformation.
        /// </summary>
        /// <value>A callback that allows pre-processing of routes before transformation.</value>
        public BeforeRouteTransformCallback? BeforeRouteTransform { get; set; }

        /// <summary>
        /// Gets or sets the callback called after route transformation.
        /// </summary>
        /// <value>A callback that allows post-processing of transformed routes.</value>
        public AfterRouteTransformCallback? AfterRouteTransform { get; set; }

        /// <summary>
        /// Creates a deep clone of <see cref="RouteConventionHooks"/> instance.
        /// </summary>
        /// <returns>A new <see cref="RouteConventionHooks"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            return new RouteConventionHooks
            {
                ShouldTransformRoute = ShouldTransformRoute,
                ShouldTransformParameter = ShouldTransformParameter,
                ShouldTransformToken = ShouldTransformToken,
                BeforeRouteTransform = BeforeRouteTransform,
                AfterRouteTransform = AfterRouteTransform,
            };
        }
    }
}
