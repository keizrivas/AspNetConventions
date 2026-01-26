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
    /// JSON output formatter that wraps responses
    /// </summary>
    public sealed class ResponseWrappingJsonFormatter(
        IOptions<AspNetConventionOptions> options,
        JsonSerializerOptions jsonSerializerOptions) : SystemTextJsonOutputFormatter(jsonSerializerOptions)
    {

        private readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

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

            // Exclude ProblemDetails types
            if (typeof(ProblemDetails).IsAssignableFrom(type))
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
