using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Http.Models;

namespace AspNetConventions.Responses.Models
{
    /// <summary>
    /// Represents a standardized response structure.
    /// </summary>
    public sealed class DefaultApiResponse(HttpStatusCode statusCode) : ApiResponse(statusCode)
    {
        /// <summary>
        /// Gets or sets the response data payload.
        /// </summary>
        /// <value>The data object to be serialized in the response. Can be null for responses without data.</value>
        [JsonPropertyOrder(5)]
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets pagination metadata for paginated responses.
        /// </summary>
        /// <value>Pagination information including page numbers, total counts, and navigation links.</value>
        /// <remarks>
        /// It's typically populated when the response contains a collection of items with pagination support.
        /// </remarks>
        [JsonPropertyOrder(7)]
        public PaginationMetadata? Pagination { get; set; }
    }
}
