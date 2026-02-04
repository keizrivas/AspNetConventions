using System;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Transformation
{
    /// <summary>
    /// Provides a route token transformer that applies ASP.NET route naming conventions to outbound route values based
    /// on configuration options.
    /// </summary>
    /// <param name="options">The options used to configure route token transformation behavior.</param>
    internal sealed class RouteTokenTransformer(IOptions<AspNetConventionOptions> options)
        : IOutboundParameterTransformer
    {
        private readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        public string? TransformOutbound(object? value)
        {
            // Check if route token transformation is enabled
            if (!_options.Value.Route.IsEnabled ||
                !_options.Value.Route.Controllers.IsEnabled ||
                !_options.Value.Route.Controllers.TransformRouteTokens ||
                value is not string token || string.IsNullOrEmpty(token))
            {
                return value?.ToString();
            }

            // Check hook to see if we should transform this token
            var shouldTransformToken = _options.Value.Route.Hooks.ShouldTransformToken
                ?.Invoke(token) ?? true;

            return !shouldTransformToken
                ? token
                : _options.Value.Route.GetCaseConverter().Convert(token);
        }
    }
}
