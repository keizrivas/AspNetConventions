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
    /// Exception handler for Minimal APIs.
    /// </summary>
    internal sealed class MinimalApiExceptionHandler(
        AspNetConventionOptions options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<MinimalApiExceptionHandler> logger) : IExceptionHandler
    {
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ILogger<MinimalApiExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var requestDescriptor = httpContext.GetRequestDescriptor();

            // Only handle JSON responses for Minimal APIs
            if (!httpContext.AcceptsJson() || requestDescriptor.EndpointType != EndpointType.MinimalApi)
            {
                return false;
            }

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
