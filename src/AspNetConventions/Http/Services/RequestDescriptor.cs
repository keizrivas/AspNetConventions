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
    /// <remarks>
    /// This class provides a comprehensive snapshot of the current HTTP request.
    /// </remarks>
    public sealed class RequestDescriptor(HttpContext httpContext)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestDescriptor"/> class with a specific status code.
        /// </summary>
        /// <param name="httpContext">The HTTP context describing the request.</param>
        /// <param name="statusCode">The HTTP status code to associate with this descriptor.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        public RequestDescriptor(HttpContext httpContext, HttpStatusCode statusCode)
            : this(httpContext)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP context containing all request and response information.
        /// </summary>
        /// <value>The current HTTP context for the request.</value>
        /// <exception cref="ArgumentNullException">Thrown when the provided HTTP context is null.</exception>
        public HttpContext HttpContext { get; } = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

        /// <summary>
        /// Gets the request path from the HTTP context.
        /// </summary>
        /// <value>The relative path of the request, excluding query string and domain.</value>
        public string? Path { get; } = httpContext.Request.Path;

        /// <summary>
        /// Gets the request path base from the HTTP context.
        /// </summary>
        /// <value>The base path for the application.</value>
        public string? PathBase { get; } = httpContext.Request.PathBase;

        /// <summary>
        /// Gets the HTTP method used for the request.
        /// </summary>
        /// <value>The HTTP verb such as GET, POST, PUT, DELETE, etc.</value>
        public string? Method { get; } = httpContext.Request.Method;

        /// <summary>
        /// Gets or sets the HTTP status code for the response.
        /// </summary>
        /// <value>The status code that will be or has been returned to the client.</value>
        /// <remarks>Initially set to the current response status code, but can be modified.</remarks>
        public HttpStatusCode StatusCode { get; private set; } = (HttpStatusCode)httpContext.Response.StatusCode;

        /// <summary>
        /// Gets the categorical type of the HTTP status code.
        /// </summary>
        /// <value>The status code classification (Informational, Success, Redirection, ClientError, ServerError).</value>
        /// <remarks>This property uses the <see cref="HttpStatusCodeExtensions.GetHttpStatusCodeType"/> extension method.</remarks>
        public HttpStatusCodeType StatusCodeType => StatusCode.GetHttpStatusCodeType();

        /// <summary>
        /// Gets the trace identifier for correlating the request across distributed systems.
        /// </summary>
        /// <value>The unique identifier for request tracing and logging.</value>
        /// <remarks>Prioritizes the current Activity ID, falling back to the HTTP context trace identifier.</remarks>
        public string? TraceId { get; } = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        /// <summary>
        /// Gets the type of endpoint handling the current request.
        /// </summary>
        /// <value>The endpoint type (MvcAction, RazorPage, MinimalApi, etc.).</value>
        /// <remarks>This property uses the <see cref="EndpointTypeDetector.GetEndpointType"/> extension method.</remarks>
        public EndpointType EndpointType { get; } = httpContext.GetEndpointType();

        /// <summary>
        /// Gets the authenticated user identifier, if authentication is enabled and the user is authenticated.
        /// </summary>
        /// <value>The user name or identifier, or null if no user is authenticated.</value>
        /// <remarks>This extracts the user identity name from the HTTP context's User property.</remarks>
        public string? UserId { get; } = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity.Name
                : null;

        /// <summary>
        /// Gets a value indicating whether the application is running in development environment.
        /// </summary>
        /// <value>true if the hosting environment is Development; otherwise, false.</value>
        /// <remarks>This is useful for conditional behavior such as including detailed error information.</remarks>
        public bool IsDevelopment { get; } = httpContext.RequestServices
                .GetService(typeof(IHostEnvironment)) is IHostEnvironment env
                && env.IsDevelopment();

        /// <summary>
        /// Gets the timestamp when the request descriptor was created.
        /// </summary>
        /// <value>The UTC timestamp representing when the request processing began.</value>
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        /// <summary>
        /// Sets the HTTP status code for the response.
        /// </summary>
        /// <param name="statusCode">The status code to set.</param>
        /// <remarks>This method allows updating the status code after the descriptor has been created.</remarks>
        public void SetStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
