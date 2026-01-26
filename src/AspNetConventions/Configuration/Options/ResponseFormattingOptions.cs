using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options.Response;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Hooks;
using AspNetConventions.Responses.Builders;

namespace AspNetConventions.Configuration.Options
{
    /// <summary>
    /// Provides configuration options for response formatting.
    /// </summary>
    public sealed class ResponseFormattingOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether response formatting is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include metadata in responses.
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        public PaginationOptions Pagination { get; set; } = new();

        public ErrorResponseOptions ErrorResponse { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of response collection adapters used to customize paginated responses.
        /// </summary>
        public HashSet<IResponseCollectionAdapter> ResponseCollectionAdapters { get; private set; } = [];

        /// <summary>
        /// Gets or sets a custom response builder.
        /// </summary>
        public IResponseBuilder? ResponseBuilder { get; set; }

        /// <summary>
        /// Gets or sets a custom error response builder.
        /// </summary>
        public IErrorResponseBuilder? ErrorResponseBuilder { get; set; }

        /// <summary>
        /// Gets or sets the collection of hooks used to customize response formatting behavior.
        /// </summary>
        public ResponseFormattingHooks Hooks { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="ResponseFormattingOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            var cloned = (ResponseFormattingOptions)MemberwiseClone();

            cloned.Pagination = (PaginationOptions)Pagination.Clone();
            cloned.ErrorResponse = (ErrorResponseOptions)ErrorResponse.Clone();
            cloned.ResponseCollectionAdapters = [..ResponseCollectionAdapters];

            return cloned;
        }

        /// <summary>
        /// Gets the response builder.
        /// </summary>
        internal IResponseBuilder GetResponseBuilder(AspNetConventionOptions options)
        {
            return ResponseBuilder
                ?? new DefaultApiResponseBuilder(options);
        }

        /// <summary>
        /// Gets the error response builder.
        /// </summary>
        internal IErrorResponseBuilder GetErrorResponseBuilder(AspNetConventionOptions options)
        {
            return ErrorResponseBuilder
                ?? new DefaultApiErrorResponseBuilder(options);
        }
    }
}
