using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetConventions.Configuration;
using AspNetConventions.Core.Abstractions.Contracts;
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
        IResponseCollectionResolver collectionResolver,
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

            var data = context.Object;
            var helper = new ResponseHelpers(collectionResolver, _options.Value, context.HttpContext);

            // Check if the response is already wrapped
            if (IsAlreadyWrapped(data, helper))
            {
                await base.WriteAsync(context).ConfigureAwait(false);
                return;
            }

            // Create http request context and attach to HttpContext items
            var requestDescriptor = context.HttpContext.ToRequestDescriptor();

            // Build the wrapped response
            var (response, statusCode) = await helper.BuildResponseAsync(data)
                .ConfigureAwait(false);

            // Create a new output formatter with the wrapped response
            var wrappedContext = new OutputFormatterWriteContext(
                context.HttpContext,
                context.WriterFactory,
                response?.GetType(),
                response);

            await base.WriteAsync(wrappedContext).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the specified type is already wrapped.
        /// </summary>
        /// <param name="data">The object to check.</param>
        /// <param name="helper">Response helper instance.</param>
        /// <returns>true if the specified context object is already wrapped; otherwise, false.</returns>
        private bool IsAlreadyWrapped(object? data, ResponseHelpers helper)
        {
            if (data == null)
            {
                return false;
            }

            var option = _options.Value;
            return helper.IsWrappedResponse(data) ||
                option.ExceptionHandling.GetResponseBuilder(option).IsWrappedResponse(data);
        }
    }
}
