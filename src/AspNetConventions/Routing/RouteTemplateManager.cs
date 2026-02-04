using System;
using System.Collections.Generic;
using System.Linq;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Hooks;
using AspNetConventions.Routing.Models;
using AspNetConventions.Routing.Parsers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing
{
    internal static class RouteTemplateManager
    {
    /// <summary>
    /// Gets the route template from the route model context, handling both MVC and Razor Page routes.
    /// </summary>
    /// <param name="model">The route model context containing route information.</param>
    /// <returns>The combined route template, or null if no template is found.</returns>
    internal static string? GetRouteTemplate(RouteModelContext model)
        {
            if(model.IsRouteEndpoint)
            {
                return model.RouteEndpointBuilder!.RoutePattern?.RawText;
            }

            var baseRoute = (model.IsAction
                    ? model.Controller!.Selectors
                    : model.Page!.Selectors)
                .Select(s => s.AttributeRouteModel)
                .FirstOrDefault(r => r != null);

            var actionRoute = model.Selector!.AttributeRouteModel;

            if (baseRoute == null || actionRoute == null)
            {
                return null;
            }

            var combined = AttributeRouteModel.CombineAttributeRouteModel(
                baseRoute,
                actionRoute);

            return combined?.Template;
        }

        /// <summary>
        /// Transforms a route template by applying case conversion to static segments.
        /// </summary>
        /// <param name="template">The route template to transform.</param>
        /// <param name="caseConverter">The case converter to apply to static segments.</param>
        /// <returns>A transformed route template with each static segment converted to the specified case.</returns>
        /// <remarks>
        /// This method:
        /// - Preserves the leading slash if present
        /// - Converts only static segments (not parameters or tokens)
        /// - Skips segments containing parameters, catch-all routes, or complex patterns
        /// - Returns the original template if it's null or empty
        /// </remarks>
        internal static string TransformRouteTemplate(string template, ICaseConverter caseConverter)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            var slashPrefix = template.StartsWith('/') ? "/" : string.Empty;
            var segments = template.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return template;
            }

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];

                // Skip parameters and token segments or invalid segments
                if (segment.StartsWith('{') || segment.StartsWith('[') ||
                    segment.IndexOf("..", StringComparison.Ordinal) > -1 ||
                    segment.IndexOf('~', StringComparison.Ordinal) > -1 ||
                    segment.IndexOf('\\', StringComparison.Ordinal) > -1)
                {
                    continue;
                }

                segments[i] = caseConverter.Convert(segment);
            }

            return $"{slashPrefix}{string.Join("/", segments)}";
        }

        /// <summary>
        /// Applies parameter name transformation to the route parameters in the specified route template.
        /// </summary>
        /// <param name="template">The route template to transform parameter names.</param>
        /// <param name="modelContext">Route model context to create parameter contexts for evaluation.</param>
        /// <param name="options">The AspNetConventions options containing transformation rules.</param>
        /// <param name="cache">Optional cache for parameter transformation decisions to improve performance.</param>
        /// <returns>A transformed route template with each parameter converted according to the configured case style.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null or whitespace.</exception>
        /// <remarks>
        /// This method:
        /// - Uses case converters to transform parameter names
        /// - Evaluates each parameter against configured hooks to determine if transformation should occur
        /// - Caches transformation decisions for performance
        /// - Preserves parameter constraints while transforming parameter names
        /// </remarks>
        internal static string TransformRouteParameters(string template,
            RouteModelContext modelContext,
            AspNetConventionOptions options,
            Dictionary<RouteParameterContext, bool>? cache = null)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException(nameof(template), "The \"template\" param can't be null or white space");
            }

            var caseConverter = options.Route.GetCaseConverter();
            return RouteParameterPatterns.ForEachParam(template, (name, constraint) =>
            {
                // Create the route key
                var paramName = RouteParameterPatterns.CleanParameterName(name);
                var parameterContext = new RouteParameterContext(modelContext, paramName);

                // Determine if parameter should be transformed
                if (cache == null || !cache.TryGetValue(parameterContext, out var shouldTransformParameter))
                {
                    shouldTransformParameter = options.Route.Hooks.ShouldTransformParameter
                        ?.Invoke(parameterContext) ?? true;

                    if(cache != null)
                    {
                        cache[parameterContext] = shouldTransformParameter;
                    }
                }

                var transformed = shouldTransformParameter
                    ? caseConverter.Convert(name)
                    : name;

                return "{" + transformed + constraint + "}";
            });
        }
    }
}
