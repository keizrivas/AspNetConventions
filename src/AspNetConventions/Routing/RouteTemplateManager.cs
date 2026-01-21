using System;
using System.Linq;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Common.Hooks;
using AspNetConventions.Routing.Models;
using AspNetConventions.Routing.Parsers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace AspNetConventions.Routing
{
    internal static class RouteTemplateManager
    {
        /// <summary>
        /// Transforms a route template by converting each static segment.
        /// </summary>
        /// <param name="model">The route template to transform.</param>
        /// <returns>A transformed route template with each static segment converted.</returns>
        internal static string? GetFullRouteTemplate(RouteModelContext model)
        {
            var baseRoute = (model.IsAction
                    ? model.Controller!.Selectors
                    : model.Page!.Selectors)
                .Select(s => s.AttributeRouteModel)
                .FirstOrDefault(r => r != null);

            var actionRoute = model.Selector.AttributeRouteModel;

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
        /// Transform parameter names in the route template.
        /// </summary>
        /// <param name="template">The route template to transform parameter names.</param>
        /// <param name="modelContext">Route model context to create the parameter context</param>
        /// <param name="caseConverter">Defines a contract for converting strings between different naming conventions.</param>
        /// <param name="callback">Optional delegate to determine if a parameter should be transformed.</param>"
        /// <returns>A transformed route template with each parameter converted.</returns>
        internal static string TransformRouteParameters(string template,
            RouteModelContext modelContext,
            ICaseConverter caseConverter,
            RouteConventionHooks.ShouldTransformParameterCallback? callback)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException(nameof(template), "The \"template\" param can't be null or white space");
            }

            return RouteParameterPatterns.ForEachParam(template, (name, constraint) =>
            {
                // Create the route key
                var paramName = RouteParameterPatterns.CleanParameterName(name);
                var parameterContext = new RouteParameterContext(modelContext, paramName);

                // Determine if parameter should be transformed
                var shouldTransformParameter = callback?
                    .Invoke(parameterContext) ?? true;

                var transformed = shouldTransformParameter
                    ? caseConverter.Convert(name)
                    : name;

                return "{" + transformed + constraint + "}";
            });
        }
    }
}
