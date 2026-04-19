using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace AspNetConventions.Routing.Transformation
{
    /// <summary>
    /// Transforms Minimal API endpoints to apply naming conventions.
    /// </summary>
    /// <remarks>
    /// This transformer applies configured route naming conventions to Minimal API endpoints.
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
        public RoutePattern? TransformRoutePattern(RouteEndpointBuilder routeEndpointBuilder)
        {
            var modelContext = RouteModelContext.FromMinimalApi(routeEndpointBuilder);
            var template = RouteTransformer.GetRouteTemplate(modelContext);

            if (string.IsNullOrEmpty(template))
            {
                return null;
            }

            // Skip templates that exceed the configured maximum length
            if (template.Length > _options.Route.MaxRouteTemplateLength)
            {
                return null;
            }

            // Skip excluded route patterns (supports * wildcard)
            if (_options.Route.MinimalApi.ExcludeRoutePatterns.Count > 0)
            {
                foreach (var pattern in _options.Route.MinimalApi.ExcludeRoutePatterns)
                {
                    if (MatchesRoutePattern(template, pattern))
                    {
                        return null;
                    }
                }
            }

            // Skip endpoints with excluded tags
            if (_options.Route.MinimalApi.ExcludeTags.Count > 0)
            {
                foreach (var metadata in routeEndpointBuilder.Metadata)
                {
                    if (metadata is ITagsMetadata tagsMetadata)
                    {
                        foreach (var tag in tagsMetadata.Tags)
                        {
                            foreach (var excluded in _options.Route.MinimalApi.ExcludeTags)
                            {
                                if (string.Equals(excluded, tag, StringComparison.OrdinalIgnoreCase))
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }

            var newTemplate = RouteTransformer.TransformRouteTemplate(template, _caseConverter);

            // Pre-populate cache with false for parameters that have an explicit binding name,
            // so both TransformRouteParameters and the segment loop skip them consistently.
            if (_options.Route.MinimalApi.TransformRouteParameters
                && _options.Route.MinimalApi.PreserveExplicitBindingNames)
            {
                foreach (var metadata in routeEndpointBuilder.Metadata)
                {
                    if (metadata is IFromRouteMetadata fromRoute
                        && !string.IsNullOrEmpty(fromRoute.Name))
                    {
                        var parameterContext = new RouteParameterContext(modelContext, fromRoute.Name);
                        _parameterTransformCache[parameterContext] = false;
                    }
                }
            }

            // Transform parameters in route
            if (options.Route.MinimalApi.TransformRouteParameters)
            {
                newTemplate = RouteTransformer.TransformRouteParameters(
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

            if (segments.Count == 0)
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

        /// <summary>
        /// Matches a route template against a pattern that may contain <c>*</c> wildcards.
        /// </summary>
        private static bool MatchesRoutePattern(string template, string pattern)
        {
            if (!pattern.Contains('*'))
            {
                return string.Equals(template, pattern, StringComparison.OrdinalIgnoreCase);
            }

            var regexPattern = "^" + Regex.Escape(pattern).Replace(@"\*", ".*") + "$";
            return Regex.IsMatch(template, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}
