using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.Http.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetConventions.ExceptionHandling.Handlers
{
    /// <summary>
    /// Default implementation of <see cref="IExceptionResponseWriter"/> that writes standardized error responses based on the configured options and exception details.
    /// </summary>
    /// <param name="options">The AspNetConventions configuration options.</param>
    /// <param name="logger">The logger instance for logging exception details and response writing process.</param>
    internal sealed class ExceptionResponseWriter(
        IOptions<AspNetConventionOptions> options,
        ILogger<GlobalExceptionHandler> logger) : ConventionOptions(options), IExceptionResponseWriter
    {
        private JsonSerializerOptions? _JsonSerializerOptions;

        private readonly ILogger<GlobalExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public ExceptionResponseWriter WithSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
        {
            _JsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
            return this;
        }

        public async ValueTask WriteResponseAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var options = CreateOptionSnapshot();

            // If exception handling is not enabled, do not
            // write a response and let the exception propagate
            if (!options.ExceptionHandling.IsEnabled)
            {
                return;
            }

            _JsonSerializerOptions ??= Options.Json.BuildSerializerOptions();
            var exceptionHandling = new ExceptionHandlingManager(httpContext, options, _logger);

            // Build the error response based on the exception and configured options
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
                _JsonSerializerOptions,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
