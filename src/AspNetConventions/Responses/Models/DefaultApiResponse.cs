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
        /// Gets or sets the response data.
        /// </summary>
        [JsonPropertyOrder(5)]
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets pagination metadata.
        /// </summary>
        [JsonPropertyOrder(7)]
        public PaginationMetadata? Pagination { get; set; }
    }
}
