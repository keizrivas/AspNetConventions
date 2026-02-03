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
        /// Transforms a route template by converting each static segment.
        /// </summary>
        /// <param name="model">The route template to transform.</param>
        /// <returns>A transformed route template with each static segment converted.</returns>
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
        /// Transforms a route template by converting each static segment.
        /// </summary>
        /// <param name="template">The route template to transform.</param>
        /// <param name="caseConverter">Defines a contract for converting strings between different naming conventions.</param>
        /// <returns>A transformed route template with each static segment converted.</returns>
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
        /// <param name="modelContext">Route model context to create the parameter context</param>
        /// <param name="options"></param>
        /// <param name="cache"></param>
        /// <returns>A transformed route template with each parameter converted.</returns>
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
