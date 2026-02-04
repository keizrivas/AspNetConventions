using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Enums;
using AspNetConventions.Extensions;
using AspNetConventions.Http.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.ExceptionHandling.Handlers
{
    /// <summary>
    /// Exception handler for Minimal APIs that provides standardized error responses.
    /// </summary>
    /// <param name="options">The AspNetConventions configuration options.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for error response formatting.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <remarks>
    /// This handler integrates with AspNetConventions to provide consistent error response formatting
    /// across Minimal API endpoints, including proper status codes, error types, messages, and metadata.
    /// </remarks>
    internal sealed class MinimalApiExceptionHandler(
        AspNetConventionOptions options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<MinimalApiExceptionHandler> logger) : IExceptionHandler
    {
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ILogger<MinimalApiExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Attempts to handle the specified exception for the Minimal API request context.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the request.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>true if the exception was handled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> or <paramref name="exception"/> is null.</exception>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var requestDescriptor = httpContext.GetRequestDescriptor();

            // Only handle JSON responses for Minimal APIs
            //if (!httpContext.AcceptsJson() || requestDescriptor.EndpointType != EndpointType.MinimalApi)
            //{
            //    return false;
            //}

            var exceptionHandling = new ExceptionHandlingManager(httpContext, _options, _logger);

            var (response, statusCode) = await exceptionHandling
                .BuildResponseFromExceptionAsync(exception, null)
                .ConfigureAwait(false);

            // Set response details
            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = ContentTypes.JsonUtf8;

            // Serialize and write the response
            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                response,
                jsonSerializerOptions,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
