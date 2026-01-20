using System;
using System.Diagnostics;
using System.Net;
using AspNetConventions.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace AspNetConventions.Http
{
    /// <summary>
    /// Describes an HTTP request and its associated metadata within the current application context.
    /// </summary>
    /// <remarks>The <see cref="RequestDescriptor"/> class provides a snapshot of key request and response information.
    /// All properties are initialized from the provided <see cref="HttpContext"/> at the time of construction and represent
    /// the state of the request at that moment.</remarks>
    public sealed class RequestDescriptor
    {
        public RequestDescriptor(HttpContext httpContext)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Path = httpContext.Request.Path;
            PathBase = httpContext.Request.PathBase;
            Method = httpContext.Request.Method;
            StatusCode = (HttpStatusCode)httpContext.Response.StatusCode;
            TraceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            EndpointType = httpContext.GetEndpointType();
            UserId = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity.Name
                : null;
            IsDevelopment = httpContext.RequestServices
                .GetService(typeof(IHostEnvironment)) is IHostEnvironment env
                && env.IsDevelopment();
            Timestamp = DateTime.UtcNow;
        }

        public RequestDescriptor(HttpContext httpContext, HttpStatusCode statusCode)
            : this(httpContext)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the request path.
        /// </summary>
        public string? Path { get; init; }

        /// <summary>
        /// Gets the request path base.
        /// </summary>
        public string? PathBase { get; init; }

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        public string? Method { get; init; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the status code type (informational, success, etc.).
        /// </summary>
        public HttpStatusCodeType StatusCodeType
        {
            get
            {
                var code = (int)StatusCode;
                return code switch
                {
                    >= 100 and <= 199 => HttpStatusCodeType.Informational,
                    >= 200 and <= 299 => HttpStatusCodeType.Success,
                    >= 300 and <= 399 => HttpStatusCodeType.Redirection,
                    >= 400 and <= 499 => HttpStatusCodeType.ClientError,
                    >= 500 and <= 599 => HttpStatusCodeType.ServerError,
                    _ => throw new InvalidOperationException("Invalid HTTP status code.")
                };
            }
        }

        /// <summary>
        /// Gets the trace identifier.
        /// </summary>
        public string? TraceId { get; init; }

        /// <summary>
        /// Gets the endpoint type.
        /// </summary>
        public EndpointType EndpointType { get; init; }

        /// <summary>
        /// Gets the authenticated user ID, if any.
        /// </summary>
        public string? UserId { get; init; }

        /// <summary>
        /// Gets whether the application is in development mode.
        /// </summary>
        public bool IsDevelopment { get; init; }

        /// <summary>
        /// Gets the timestamp when the request was processed.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Sets the status code.
        /// </summary>
        public void SetStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
