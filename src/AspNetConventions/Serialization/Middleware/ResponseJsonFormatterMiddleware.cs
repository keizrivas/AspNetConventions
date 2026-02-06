using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Serialization.Middleware
{
    /// <summary>
    /// Middleware that applies JSON serialization conventions to HTTP responses.
    /// </summary>
    /// <remarks>
    /// This middleware intercepts JSON responses and reserializes them using configured
    /// JSON serialization options. It ensures that all JSON responses conform to the
    /// naming conventions and serialization rules defined in the AspNetConventions configuration.
    /// The middleware only processes responses that accept JSON content type.
    /// </remarks>
    public sealed class ResponseJsonFormatterMiddleware
    {
        /// <summary>
        /// The next middleware in the request pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The JSON serializer options to apply to responses.
        /// </summary>
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// The logger for diagnostic information.
        /// </summary>
        private readonly ILogger<ResponseJsonFormatterMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseJsonFormatterMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the request pipeline.</param>
        /// <param name="jsonOptions">The JSON serializer options to apply to responses.</param>
        /// <param name="logger">The logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public ResponseJsonFormatterMiddleware(
            RequestDelegate next,
            JsonSerializerOptions jsonOptions,
            ILogger<ResponseJsonFormatterMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware to process the HTTP request and response.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task that represents the asynchronous middleware operation.</returns>
        /// <remarks>
        /// This method:
        /// 1. Captures the response body in a memory stream
        /// 2. Invokes the next middleware in the pipeline
        /// 3. Checks if the response accepts JSON content type
        /// 4. If JSON, reserializes the response using configured options
        /// 5. Otherwise, copies the original response as-is
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            //// Check if this is a JSON response
            //if (!ShouldProcessRequest(context))
            //{
            //    await _next(context);
            //    return;
            //}

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Only process JSON responses
                if (context.AcceptsJson())
                {
                    await ReserializeJsonAsync(context, responseBody, originalBodyStream);
                }
                else
                {
                    // Copy as-is
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream, context.RequestAborted);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

/// <summary>
        /// Reserializes JSON response content using configured serialization options.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="responseBody">The memory stream containing the original response.</param>
        /// <param name="originalBodyStream">The original response body stream to write to.</param>
        /// <returns>A task that represents the asynchronous reserialization operation.</returns>
        /// <remarks>
        /// This method:
        /// 1. Deserializes the existing JSON using default options
        /// 2. Reserializes it using the configured naming conventions
        /// 3. Handles JSON serialization errors gracefully by falling back to original content
        /// </remarks>
        private async Task ReserializeJsonAsync(
            HttpContext context,
            MemoryStream responseBody,
            Stream originalBodyStream)
        {
            if (responseBody.Length == 0)
            {
                return;
            }

            responseBody.Seek(0, SeekOrigin.Begin);

            try
            {
                // Deserialize with default options
                var data = await JsonSerializer.DeserializeAsync<JsonElement>(
                    responseBody,
                    cancellationToken: context.RequestAborted);

                // Re-serialize with our conventions
                context.Response.ContentLength = null;
                await JsonSerializer.SerializeAsync(
                    originalBodyStream,
                    data,
                    _jsonOptions,
                    cancellationToken: context.RequestAborted);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to reserialize JSON response");

                // Fall back to original response
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream, context.RequestAborted);
            }
        }
    }
}
