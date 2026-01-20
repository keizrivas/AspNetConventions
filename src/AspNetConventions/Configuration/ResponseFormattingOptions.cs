using System;
using System.Collections.Generic;
using AspNetConventions.Common.Hooks;
using AspNetConventions.ResponseFormatting.Abstractions;
using AspNetConventions.ResponseFormatting.Builders;
using AspNetConventions.ResponseFormatting.Enums;

namespace AspNetConventions.Configuration
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
        /// Gets or sets the response formatting style.
        /// </summary>
        public ResponseStyle Style { get; set; } = ResponseStyle.Standard;

        /// <summary>
        /// Gets or sets whether to include metadata in responses.
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include pagination metadata.
        /// </summary>
        public bool IncludePaginationMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include pagination links.
        /// </summary>
        public bool IncludePaginationLinks { get; set; } = true;

        /// <summary>
        /// Gets or sets the query parameter name for the page number.
        /// </summary>
        public string PageNumberQueryParameterName { get; set; } = "page";

        /// <summary>
        /// Gets or sets the query parameter name for the page size.
        /// </summary>
        public string PageSizeQueryParameterName { get; set; } = "pageSize";

        /// <summary>
        /// Gets or sets the default page size.
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Gets or sets the collection of response collection adapters used to customize paginated responses.
        /// </summary>
        public IList<IResponseCollectionAdapter> ResponseCollectionAdapters { get; private set; } = [];

        /// <summary>
        /// Gets or sets the collection of hooks used to customize response formatting behavior.
        /// </summary>
        public ResponseFormattingHooks Hooks { get; set; } = new();

        /// <summary>
        /// Gets or sets a custom response builder.
        /// </summary>
        public IResponseBuilder? CustomResponseBuilder { get; set; }

        /// <summary>
        /// Creates a deep clone of <see cref="ResponseFormattingOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            return new ResponseFormattingOptions
            {
                IsEnabled = IsEnabled,
                Style = Style,
                IncludeMetadata = IncludeMetadata,
                IncludePaginationMetadata = IncludePaginationMetadata,
                IncludePaginationLinks = IncludePaginationLinks,
                PageNumberQueryParameterName = PageNumberQueryParameterName,
                PageSizeQueryParameterName = PageSizeQueryParameterName,
                DefaultPageSize = DefaultPageSize,
                ResponseCollectionAdapters = ResponseCollectionAdapters,
                Hooks = Hooks,
                CustomResponseBuilder = CustomResponseBuilder,
            };
        }

        /// <summary>
        /// Gets the response builder.
        /// </summary>
        internal IResponseBuilder GetResponseBuilder(AspNetConventionOptions options)
        {
            var standardResponse = new StandardResponseBuilder(options);

            return Style switch
            {
                ResponseStyle.Standard => standardResponse,
                ResponseStyle.Custom => CustomResponseBuilder
                    ?? throw new InvalidOperationException("CustomResponseBuilder not set"),
                _ => standardResponse
            };
        }
    }
}
