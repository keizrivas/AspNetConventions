using System;
using System.Diagnostics;
using System.Net;
using AspNetConventions.Core.Enums;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace AspNetConventions.Http.Services
{
    /// <summary>
    /// Describes an HTTP request and its associated metadata within the current application context.
    /// </summary>
    public sealed class RequestDescriptor(HttpContext httpContext)
    {
        public RequestDescriptor(HttpContext httpContext, HttpStatusCode statusCode)
            : this(httpContext)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; } = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

        /// <summary>
        /// Gets the request path.
        /// </summary>
        public string? Path { get; } = httpContext.Request.Path;

        /// <summary>
        /// Gets the request path base.
        /// </summary>
        public string? PathBase { get; } = httpContext.Request.PathBase;

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        public string? Method { get; } = httpContext.Request.Method;

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; } = (HttpStatusCode)httpContext.Response.StatusCode;

        /// <summary>
        /// Gets the status code type (informational, success, etc.).
        /// </summary>
        public HttpStatusCodeType StatusCodeType => StatusCode.GetHttpStatusCodeType();

        /// <summary>
        /// Gets the trace identifier.
        /// </summary>
        public string? TraceId { get; } = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        /// <summary>
        /// Gets the endpoint type.
        /// </summary>
        public EndpointType EndpointType { get; } = httpContext.GetEndpointType();

        /// <summary>
        /// Gets the authenticated user ID, if any.
        /// </summary>
        public string? UserId { get; } = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity.Name
                : null;

        /// <summary>
        /// Gets whether the application is in development mode.
        /// </summary>
        public bool IsDevelopment { get; } = httpContext.RequestServices
                .GetService(typeof(IHostEnvironment)) is IHostEnvironment env
                && env.IsDevelopment();

        /// <summary>
        /// Gets the timestamp when the request was processed.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        /// <summary>
        /// Sets the status code.
        /// </summary>
        public void SetStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
