using System;
using System.IO;
using AspNetConventions.Core.Enums;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace AspNetConventions.Http.Services
{
    /// <summary>
    /// Provides extension methods for detecting the type of ASP.NET Core endpoint from HTTP context.
    /// </summary>
    /// <remarks>
    /// This class analyzes the current HTTP request context to determine what type of endpoint
    /// is being processed. It supports detection of MVC controllers, Razor Pages, Minimal APIs,
    /// Blazor components, health checks, SignalR hubs, and static files.
    /// </remarks>
    internal static class EndpointTypeDetector
    {
        /// <summary>
        /// Gets the endpoint type from the current HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing endpoint information.</param>
        /// <returns>The detected <see cref="EndpointType"/> of the current request.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
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
                return EndpointType.MvcAction;

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
        /// Determines if the current context represents an API endpoint.
        /// </summary>
        /// <param name="httpContext">The HTTP context to analyze.</param>
        /// <returns>true if the endpoint is an MVC action or Minimal API; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        /// <remarks>
        /// API endpoints are typically those that return data rather than UI content.
        /// This method identifies both traditional MVC controller actions and modern Minimal APIs.
        /// </remarks>
        public static bool IsApiEndpoint(this HttpContext httpContext)
        {
            var endpointType = httpContext.GetEndpointType();
            return endpointType == EndpointType.MvcAction ||
                   endpointType == EndpointType.MinimalApi;
        }

        /// <summary>
        /// Determines if the current context represents a UI endpoint.
        /// </summary>
        /// <param name="httpContext">The HTTP context to analyze.</param>
        /// <returns>true if the endpoint is a Razor Page or Blazor component; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        /// <remarks>
        /// UI endpoints are those that render user interface content rather than returning data.
        /// This method identifies both Razor Pages and Blazor components as UI endpoints.
        /// </remarks>
        public static bool IsUIEndpoint(this HttpContext httpContext)
        {
            var endpointType = httpContext.GetEndpointType();
            return endpointType == EndpointType.RazorPage ||
                   endpointType == EndpointType.Blazor;
        }

        /// <summary>
        /// Determines if the file path represents a static file based on its extension.
        /// </summary>
        /// <param name="path">The file path to analyze.</param>
        /// <returns>true if the path has a static file extension; otherwise, false.</returns>
        /// <remarks>
        /// This method checks for common static file extensions including:
        /// - Stylesheets (.CSS)
        /// - JavaScript files (.JS, .MAP)
        /// - Images (.JPG, .JPEG, .PNG, .GIF, .SVG, .ICO)
        /// - Fonts (.WOFF, .WOFF2, .TTF, .EOT)
        /// - Data files (.JSON, .XML)
        /// </remarks>
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
