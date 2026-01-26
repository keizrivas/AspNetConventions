using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Abstractions.Models;

namespace AspNetConventions.Responses.Models
{
    /// <summary>
    /// Represents a standardized error response structure.
    /// </summary>
    public sealed class DefaultApiErrorResponse<TError> : ApiResponse
    {
        public DefaultApiErrorResponse(HttpStatusCode statusCode, object? errors = null) : base(statusCode)
        {
            if (errors is null)
                Errors = [];

            else if (errors is IReadOnlyCollection<TError> collection)
                Errors = collection;

            else if (errors is IEnumerable<TError> enumerable)
                Errors = [.. enumerable];

            else if (errors is TError single)
                Errors = [single];
        }

        /// <summary>
        /// Gets or sets an type.
        /// </summary>
        [JsonPropertyOrder(3)]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the errors list.
        /// </summary>
        [JsonPropertyOrder(5)]
        public IReadOnlyCollection<TError> Errors { get; init; } = [];
    }
}
