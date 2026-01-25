using System;
using AspNetConventions.Routing.Models;

namespace AspNetConventions.Core.Hooks
{
    public class RouteConventionHooks: ICloneable
    {
        public delegate bool ShouldTransformRouteCallback(string template, RouteModelContext model);
        public delegate bool ShouldTransformParameterCallback(RouteParameterContext model);
        public delegate bool ShouldTransformTokenCallback(string token);
        public delegate void BeforeRouteTransformCallback(string route, RouteModelContext model);
        public delegate void AfterRouteTransformCallback(string route, string originalRoute, RouteModelContext model);

        /// <summary>
        /// Called before parameter binding configuration
        /// Return false to skip this parameter
        /// </summary>
        public ShouldTransformRouteCallback? ShouldTransformRoute { get; set; }

        /// <summary>
        /// Called before parameter binding configuration
        /// Return false to skip this parameter
        /// </summary>
        public ShouldTransformParameterCallback? ShouldTransformParameter { get; set; }

        /// <summary>
        /// Called before transforming a route token
        /// Return false to skip this token
        /// </summary>
        public ShouldTransformTokenCallback? ShouldTransformToken { get; set; }

        /// <summary>
        /// Called before route transformation
        /// Return modified template or null to skip transformation
        /// </summary>
        public BeforeRouteTransformCallback? BeforeRouteTransform { get; set; }

        /// <summary>
        /// Called after route transformation
        /// Allows post-processing of transformed routes
        /// </summary>
        public AfterRouteTransformCallback? AfterRouteTransform { get; set; }

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
