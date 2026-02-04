using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace AspNetConventions.Routing.Transformation
{
    /// <summary>
    /// Transforms Minimal API endpoints to apply naming conventions.
    /// </summary>
    /// <remarks>
    /// This transformer applies configured route naming conventions to Minimal API endpoints, including
    /// case conversion for route segments and parameters, while supporting exclusion rules and
    /// hooks for customization.
    /// </remarks>
    internal sealed class EndpointTransformer(AspNetConventionOptions options)
    {
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ICaseConverter _caseConverter = options.Route.GetCaseConverter();
        private static readonly Dictionary<RouteParameterContext, bool> _parameterTransformCache = [];

        /// <summary>
        /// Transforms a route endpoint's pattern according to configured conventions.
        /// </summary>
        /// <param name="routeEndpointBuilder">The route endpoint builder to transform.</param>
        /// <returns>The transformed route pattern, or null if no transformation was applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="routeEndpointBuilder"/> is null.</exception>
        /// <remarks>
        /// Returns null if the pattern is unchanged or transformation is disabled
        /// </remarks>
        public RoutePattern? TransformRoutePattern(RouteEndpointBuilder routeEndpointBuilder)
        {
            var modelContext = RouteModelContext.FromMinimalApi(routeEndpointBuilder);
            var template = RouteTemplateManager.GetRouteTemplate(modelContext);

            if (string.IsNullOrEmpty(template))
            {
                return null;
            }

            // Determine if route should be transformed
            var shouldTransformRoute = _options.Route.Hooks.ShouldTransformRoute
                ?.Invoke(template, modelContext) ?? true;

            if (!shouldTransformRoute)
            {
                return null;
            }

            var newTemplate = RouteTemplateManager.TransformRouteTemplate(template, _caseConverter);

            // Transform parameters in route
            if (options.Route.MinimalApi.TransformRouteParameters)
            {
                newTemplate = RouteTemplateManager.TransformRouteParameters(
                    newTemplate,
                    modelContext,
                    options,
                    _parameterTransformCache);
            }

            var segments = new List<RoutePatternPathSegment>();
            foreach (var segment in routeEndpointBuilder.RoutePattern.PathSegments)
            {
                var newParts = new List<RoutePatternPart>();
                foreach (var part in segment.Parts)
                {
                    if (part is RoutePatternLiteralPart literal)
                    {
                        newParts.Add(RoutePatternFactory.LiteralPart(_caseConverter.Convert(literal.Content)));
                    }
                    else if (part is RoutePatternParameterPart parameterPart && _options.Route.MinimalApi.TransformRouteParameters)
                    {
                        // Determine if parameter should be transformed
                        var parameterContext = new RouteParameterContext(modelContext, parameterPart.Name);

                        if (!_parameterTransformCache.TryGetValue(parameterContext, out var shouldTransformParameter))
                        {
                            shouldTransformParameter = _options.Route.Hooks.ShouldTransformParameter
                                ?.Invoke(parameterContext) ?? true;

                            _parameterTransformCache[parameterContext] = shouldTransformParameter;
                        }

                        if (shouldTransformParameter)
                        {
                            newParts.Add(RoutePatternFactory.ParameterPart(
                                _caseConverter.Convert(parameterPart.Name),
                                parameterPart.Default,
                                parameterPart.ParameterKind,
                                parameterPart.ParameterPolicies));
                        }
                        else
                        {
                            newParts.Add(part);
                        }
                    }
                    else
                    {
                        newParts.Add(part);
                    }
                }

                segments.Add(RoutePatternFactory.Segment(newParts));
            }

            if(segments.Count == 0)
            {
                return null;
            }

            var newPattern = RoutePatternFactory.Pattern(newTemplate, segments);

            // If pattern unchanged, return original
            if (newPattern == null || string.Equals(template, newPattern.RawText, StringComparison.Ordinal))
            {
                return null;
            }
            
            _options.Route.Hooks.BeforeRouteTransform?.Invoke(template, modelContext);
            routeEndpointBuilder.RoutePattern = newPattern;
            _options.Route.Hooks.AfterRouteTransform?.Invoke(newTemplate, template, modelContext);

            return newPattern;
        }
    }
}
