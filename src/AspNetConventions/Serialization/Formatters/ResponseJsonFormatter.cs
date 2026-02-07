using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Extensions;
using AspNetConventions.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Serialization.Formatters
{
    /// <summary>
    /// JSON output formatter that wraps responses according to AspNetConventions standards.
    /// </summary>
    /// <param name="options">The AspNetConventions configuration options.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for output formatting.</param>
    /// <remarks>
    /// This formatter automatically wraps API responses with standardized structure including metadata,
    /// pagination information, and error details when applicable.
    /// </remarks>
    public sealed class ResponseJsonFormatter(
        IOptions<AspNetConventionOptions> options,
        JsonSerializerOptions jsonSerializerOptions) : SystemTextJsonOutputFormatter(jsonSerializerOptions)
    {

        private readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Determines whether the formatter can write the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the formatter can write the type; otherwise, false.</returns>
        protected override bool CanWriteType(Type? type)
        {
            if (!_options.Value.Response.IsEnabled)
            {
                return false;
            }

            // Exclude Stream types
            if (typeof(Stream).IsAssignableFrom(type))
            {
                return false;
            }

            // Exclude Streaming JSON types
            if ((type != null && type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)) ||
                typeof(IAsyncEnumerable<>).IsAssignableFrom(type))
            {
                return false;
            }

            return base.CanWriteType(type);
        }

        /// <summary>
        /// Writes the response object to the output stream with automatic response wrapping.
        /// </summary>
        /// <param name="context">The output formatter write context.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when response wrapping fails due to configuration issues.</exception>
        public override async Task WriteAsync(OutputFormatterWriteContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var payload = context.Object;

            // Create http request context and attach to HttpContext items
            var requestDescriptor = context.HttpContext.GetRequestDescriptor();

            // TODO: Logger should be injected via DI
            var responseManager = new ResponseManager(_options.Value, requestDescriptor, null);

            // Check if the response is already wrapped
            if (responseManager.IsWrappedResponse(payload))
            {
                await base.WriteAsync(context).ConfigureAwait(false);
                return;
            }

            // Build the wrapped response
            var (response, statusCode) = await responseManager.BuildResponseAsync(payload)
                .ConfigureAwait(false);

            // Create a new output formatter with the wrapped response
            var wrappedContext = new OutputFormatterWriteContext(
                context.HttpContext,
                context.WriterFactory,
                response?.GetType(),
                response);

            await base.WriteAsync(wrappedContext).ConfigureAwait(false);
        }
    }
}
