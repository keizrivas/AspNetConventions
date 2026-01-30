using System;
using System.Linq;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Routing.Models;
using AspNetConventions.Routing.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Transformation
{
    /// <summary>
    /// Transforms Minimal API endpoints to apply naming conventions.
    /// </summary>
    internal sealed class EndpointTransformer(AspNetConventionOptions options)
    {
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ICaseConverter _caseConverter = options.Route.GetCaseConverter();

        /// <summary>
        /// Transforms a route endpoint's pattern.
        /// </summary>
        public bool TransformEndpoint(RouteEndpointBuilder routeEndpointBuilder)
        {
            var template = RouteTemplateManager.GetRouteTemplate(routeEndpointBuilder);
            if(template is null)
            {
                return true;
            }

            var modelContext = new RouteModelContext(routeEndpointBuilder);
            var newTemplate = RouteTemplateManager.TransformRouteTemplate(template, _caseConverter);

            // Transform parameters in route
            if (_options.Route.MinimalApi.TransformRouteParameters)
            {
                newTemplate = RouteTemplateManager.TransformRouteParameters(
                    newTemplate,
                    modelContext,
                    _caseConverter,
                    _options.Route.Hooks.ShouldTransformParameter);
            }

            // If pattern unchanged, return original
            if (string.Equals(template, newTemplate, StringComparison.Ordinal))
            {
                return false;
            }

            // Create ans set the new route pattern
            var newPattern = RoutePatternFactory.Parse(newTemplate);
            routeEndpointBuilder.RoutePattern = newPattern;

            //// Copy metadata
            //var metadata = routeEndpointBuilder.Metadata.ToArray();

            //// Copy request delegate
            //var requestDelegate = routeEndpointBuilder.RequestDelegate
            //    ?? (context => context.Response.CompleteAsync());

            //// Build new endpoint
            //return new RouteEndpoint(
            //    requestDelegate,
            //    newPattern,
            //    routeEndpointBuilder.Order,
            //    new EndpointMetadataCollection(metadata),
            //    routeEndpointBuilder.DisplayName);

            return true;
        }
    }
}
