using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Abstractions.Models;

namespace AspNetConventions.Responses.Models
{
    /// <summary>
    /// Represents a standardized error response structure.
    /// </summary>
    public sealed class DefaultApiErrorResponse(HttpStatusCode statusCode) : ApiResponse(statusCode)
    {
        /// <summary>
        /// Gets or sets an type.
        /// </summary>
        [JsonPropertyOrder(3)]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the errors list.
        /// </summary>
        [JsonPropertyOrder(5)]
        public HashSet<object> Errors { get; } = [];
    }
}
