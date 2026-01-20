using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Http;

namespace AspNetConventions.ResponseFormatting.Models
{
    /// <summary>
    /// Represents a standardized response structure.
    /// </summary>
    public sealed class StandardResponse(HttpStatusCode statusCode) : BaseResponse(statusCode)
    {
        /// <summary>
        /// Gets or sets pagination metadata.
        /// </summary>
        [JsonPropertyOrder(5)]
        public PaginationMetadata? Pagination { get; set; }
    }
}
