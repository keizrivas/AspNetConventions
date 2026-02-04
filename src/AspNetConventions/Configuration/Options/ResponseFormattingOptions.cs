using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options.Response;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Hooks;
using AspNetConventions.Responses.Builders;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Configuration.Options
{
    /// <summary>
    /// Provides configuration options for response formatting in AspNetConventions.
    /// </summary>
    /// <remarks>
    /// These options control how responses are structured, including metadata inclusion,
    /// pagination behavior, error response and custom response builders.
    /// </remarks>
    public sealed class ResponseFormattingOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether response formatting is enabled.
        /// </summary>
        /// <value>Default is true. When disabled, responses will not be wrapped or processed by AspNetConventions.</value>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include metadata in responses.
        /// </summary>
        /// <value>Default is true. When enabled, responses include request metadata such as endpoint type, HTTP method, and path.</value>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets the pagination configuration options.
        /// </summary>
        /// <value>Default is a new <see cref="PaginationOptions"/> instance. Controls how paginated responses are formatted.</value>
        public PaginationOptions Pagination { get; set; } = new();

        /// <summary>
        /// Gets or sets the error response configuration options.
        /// </summary>
        /// <value>Default is a new <see cref="ErrorResponseOptions"/> instance. Controls how error responses are formatted.</value>
        public ErrorResponseOptions ErrorResponse { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of response collection adapters used to customize paginated responses.
        /// </summary>
        /// <value>A set of adapters that can customize how different collection types are handled in paginated responses.</value>
        /// <remarks>Adapters allow you to customize how specific collection types are resolved and formatted.</remarks>
        public HashSet<IResponseCollectionAdapter> ResponseCollectionAdapters { get; private set; } = [];

        /// <summary>
        /// Gets or sets a custom response builder for successful responses.
        /// </summary>
        /// <value>When null, the default <see cref="DefaultApiResponseBuilder"/> will be used.</value>
        /// <remarks>Use this to customize how successful responses are structured and formatted.</remarks>
        public IResponseBuilder? ResponseBuilder { get; set; }

        /// <summary>
        /// Gets or sets a custom error response builder for error responses.
        /// </summary>
        /// <value>When null, the default <see cref="DefaultApiErrorResponseBuilder"/> will be used.</value>
        /// <remarks>Use this to customize how error responses are structured and formatted.</remarks>
        public IErrorResponseBuilder? ErrorResponseBuilder { get; set; }

        /// <summary>
        /// Gets or sets the collection of hooks used to customize response formatting behavior.
        /// </summary>
        /// <value>Hooks allow you to intercept and customize the response formatting process at various stages.</value>
        public ResponseFormattingHooks Hooks { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="ResponseFormattingOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ResponseFormattingOptions"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            var cloned = (ResponseFormattingOptions)MemberwiseClone();

            cloned.Pagination = (PaginationOptions)Pagination.Clone();
            cloned.ErrorResponse = (ErrorResponseOptions)ErrorResponse.Clone();
            cloned.ResponseCollectionAdapters = [..ResponseCollectionAdapters];

            return cloned;
        }

        /// <summary>
        /// Gets the response builder instance, using the custom builder if configured or the default builder otherwise.
        /// </summary>
        /// <param name="options">The main AspNetConventions options.</param>
        /// <param name="logger">The logger for diagnostic information.</param>
        /// <returns>An instance of <see cref="IResponseBuilder"/> for formatting successful responses.</returns>
        /// <remarks>If no custom builder is configured, returns a <see cref="DefaultApiResponseBuilder"/> instance.</remarks>
        internal IResponseBuilder GetResponseBuilder(AspNetConventionOptions options, ILogger logger)
        {
            return ResponseBuilder
                ?? new DefaultApiResponseBuilder(options, logger);
        }

        /// <summary>
        /// Gets the error response builder instance, using the custom builder if configured or the default builder otherwise.
        /// </summary>
        /// <param name="options">The main AspNetConventions options.</param>
        /// <param name="logger">The logger for diagnostic information.</param>
        /// <returns>An instance of <see cref="IErrorResponseBuilder"/> for formatting error responses.</returns>
        /// <remarks>If no custom builder is configured, returns a <see cref="DefaultApiErrorResponseBuilder"/> instance.</remarks>
        internal IErrorResponseBuilder GetErrorResponseBuilder(AspNetConventionOptions options, ILogger logger)
        {
            return ErrorResponseBuilder
                ?? new DefaultApiErrorResponseBuilder(options, logger);
        }
    }
}
