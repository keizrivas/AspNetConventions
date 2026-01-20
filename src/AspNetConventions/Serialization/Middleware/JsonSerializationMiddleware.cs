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
    /// Middleware that applies JSON serialization conventions to responses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This middleware ensures all JSON responses follow configured naming conventions
    /// (e.g., snake_case, camelCase) regardless of how they were created.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> Place this middleware after response formatting but before
    /// any middleware that writes to the response body.
    /// </para>
    /// </remarks>
    public sealed class JsonSerializationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<JsonSerializationMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationMiddleware"/> class.
        /// </summary>
        public JsonSerializationMiddleware(
            RequestDelegate next,
            JsonSerializerOptions jsonOptions,
            ILogger<JsonSerializationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
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
