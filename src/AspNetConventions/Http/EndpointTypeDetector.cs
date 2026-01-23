using System;
using System.IO;
using AspNetConventions.Common.Enums;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace AspNetConventions.Http
{
    /// <summary>
    /// Detects the type of ASP.NET Core endpoint.
    /// </summary>
    internal static class EndpointTypeDetector
    {
        /// <summary>
        /// Gets the endpoint type from HTTP context.
        /// </summary>
        public static EndpointType GetEndpointType(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var endpoint = httpContext.GetEndpoint();
            if (endpoint == null)
                return EndpointType.Unknown;

            // Check for Razor Pages
            if (endpoint.Metadata.GetMetadata<PageActionDescriptor>() != null)
                return EndpointType.RazorPage;

            // Check for MVC Controllers
            if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() != null)
                return EndpointType.MvcController;

            // Check for Blazor
            if (endpoint.Metadata.GetMetadata<ComponentTypeMetadata>() != null ||
                endpoint.DisplayName?.Contains("Blazor", StringComparison.OrdinalIgnoreCase) == true ||
                httpContext.Request.Path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase))
            {
                return EndpointType.Blazor;
            }

            // Check for Health Checks
            if (endpoint.Metadata.GetMetadata<HealthCheckOptions>() != null ||
                endpoint.DisplayName?.Contains("Health checks", StringComparison.OrdinalIgnoreCase) == true)
            {
                return EndpointType.HealthCheck;
            }

            // Check for SignalR
            if (httpContext.Request.Path.StartsWithSegments("/hub", StringComparison.OrdinalIgnoreCase) ||
                endpoint.DisplayName?.Contains("SignalR", StringComparison.OrdinalIgnoreCase) == true)
            {
                return EndpointType.SignalR;
            }

            // Check for Minimal APIs
            if (endpoint is RouteEndpoint routeEndpoint &&
                routeEndpoint.Metadata.GetMetadata<ControllerActionDescriptor>() == null &&
                routeEndpoint.Metadata.GetMetadata<PageActionDescriptor>() == null)
            {
                return EndpointType.MinimalApi;
            }

            // Check for static files
            if (httpContext.Request.Path.HasValue &&
                IsStaticFileExtension(httpContext.Request.Path.Value))
            {
                return EndpointType.StaticFiles;
            }

            return EndpointType.Unknown;
        }

        /// <summary>
        /// Determines if the context is an API endpoint.
        /// </summary>
        public static bool IsApiEndpoint(this HttpContext httpContext)
        {
            var endpointType = httpContext.GetEndpointType();
            return endpointType == EndpointType.MvcController ||
                   endpointType == EndpointType.MinimalApi;
        }

        /// <summary>
        /// Determines if the context is a UI endpoint.
        /// </summary>
        public static bool IsUIEndpoint(this HttpContext httpContext)
        {
            var endpointType = httpContext.GetEndpointType();
            return endpointType == EndpointType.RazorPage ||
                   endpointType == EndpointType.Blazor;
        }

        private static bool IsStaticFileExtension(string path)
        {
            var extension = Path.GetExtension(path).ToUpperInvariant();

            return extension switch
            {
                ".CSS" or ".JS" or ".JPG" or ".JPEG" or ".PNG" or ".GIF" or
                ".SVG" or ".ICO" or ".WOFF" or ".WOFF2" or ".TTF" or ".EOT" or
                ".MAP" or ".JSON" or ".XML" => true,
                _ => false
            };
        }
    }
}
