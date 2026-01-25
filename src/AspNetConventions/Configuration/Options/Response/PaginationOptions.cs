using System;

namespace AspNetConventions.Configuration.Options.Response
{
    public sealed class PaginationOptions : ICloneable
    {
        /// <summary>
        /// Include pagination metadata in responses
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Include pagination links (first, last, next, prev)
        /// </summary>
        public bool IncludeLinks { get; set; } = true;

        /// <summary>
        /// Query parameter name for page number
        /// </summary>
        public string PageNumberParameterName { get; set; } = "page";

        /// <summary>
        /// Query parameter name for page size
        /// </summary>
        public string PageSizeParameterName { get; set; } = "pageSize";

        /// <summary>
        /// Default page size when not specified
        /// </summary>
        public int DefaultPageSize { get; set; } = 20;

        public object Clone() => MemberwiseClone();
    }
}
