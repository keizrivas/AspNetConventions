using System;
using System.Linq;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Routing.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace AspNetConventions.Routing.Transformation
{
    /// <summary>
    /// Transforms Minimal API endpoints to apply naming conventions.
    /// </summary>
    internal sealed class EndpointTransformer
    {
        private readonly RouteConventionOptions _options;
        private readonly ICaseConverter _caseConverter;

        public EndpointTransformer(RouteConventionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _caseConverter = options.GetCaseConverter();
        }

        /// <summary>
        /// Transforms a route endpoint's pattern.
        /// </summary>
        public Endpoint TransformEndpoint(RouteEndpoint original)
        {
            var rawText = original.RoutePattern?.RawText
                ?? original.DisplayName
                ?? string.Empty;

            var transformedText = TransformRoutePattern(rawText);

            // If pattern unchanged, return original
            if (string.Equals(rawText, transformedText, StringComparison.Ordinal))
                return original;

            // Create new route pattern
            var newPattern = RoutePatternFactory.Parse(transformedText);

            // Copy metadata
            var metadata = original.Metadata.ToArray();

            // Copy request delegate
            var requestDelegate = original.RequestDelegate
                ?? (context => context.Response.CompleteAsync());

            // Build new endpoint
            return new RouteEndpoint(
                requestDelegate,
                newPattern,
                original.Order,
                new EndpointMetadataCollection(metadata),
                original.DisplayName);
        }

        private string TransformRoutePattern(string rawTemplate)
        {
            if (string.IsNullOrWhiteSpace(rawTemplate))
                return rawTemplate;

            var segments = rawTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];

                if (segment.StartsWith('{'))
                {
                    // Parameter segment
                    if (_options.MinimalApi.TransformRouteParameters)
                    {
                        segments[i] = TransformParameter(segment);
                    }
                }
                else
                {
                    // Regular segment
                    segments[i] = _caseConverter.Convert(segment);
                }
            }

            return "/" + string.Join("/", segments);
        }

        private string TransformParameter(string segment)
        {
            return RouteParameterPatterns.ExtractParameterNameWithMarkersAndConstraints().Replace(segment, m =>
            {
                var name = m.Groups["name"].Value;
                var constraint = m.Groups["constraint"].Value;
                var transformed = _caseConverter.Convert(name);
                return "{" + transformed + constraint + "}";
            });
        }
    }
}
