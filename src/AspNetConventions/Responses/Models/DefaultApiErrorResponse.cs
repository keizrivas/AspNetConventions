using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Abstractions.Models;

namespace AspNetConventions.Responses.Models
{
    /// <summary>
    /// Represents a standardized error response structure.
    /// </summary>
    /// <typeparam name="TError">The type of error details included in the response.</typeparam>
    public sealed class DefaultApiErrorResponse<TError> : ApiResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiErrorResponse{TError}"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the error response.</param>
        /// <param name="errors">The error details to include in the response. Can be a single error, collection, or null.</param>
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
        /// Gets or sets the error type classification.
        /// </summary>
        /// <value>A string representing the category or type of error that occurred.</value>
        [JsonPropertyOrder(3)]
        public required string Type { get; set; }

        /// <summary>
        /// Gets the collection of error details.
        /// </summary>
        /// <value>A read-only collection containing the specific error information.</value>
        [JsonPropertyOrder(5)]
        public IReadOnlyCollection<TError> Errors { get; init; } = [];
    }
}
