using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Abstractions.Models;

namespace AspNetConventions.Responses.Models
{
    /// <summary>
    /// Represents a standardized error response structure.
    /// </summary>
    public sealed class DefaultApiErrorResponse : ApiResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiErrorResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the error response.</param>
        /// <param name="errors">The error details to include in the response. Can be a single error, collection, or null.</param>
        public DefaultApiErrorResponse(HttpStatusCode statusCode, object? errors = null) : base(statusCode)
        {
            Errors = NormalizeErrors(errors);
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
        public IReadOnlyCollection<object> Errors { get; }

        /// <summary>
        /// Normalizes the specified error object into a read-only collection of error items.
        /// </summary>
        /// <param name="errors">An object representing one or more errors. Can be null, a single error, a collection of errors, or a
        /// dictionary representing a structured error.</param>
        /// <returns>A read-only collection of error objects. Returns an empty collection if the input is null.</returns>
        private static IReadOnlyCollection<object> NormalizeErrors(object? errors)
        {
            // Empty error
            if (errors is null)
            {
                return [];
            }

            // Read only collections
            if (errors is IReadOnlyCollection<object> readOnlyCollection)
            {
                return readOnlyCollection;
            }

            // Dictionaries
            if (errors is IDictionary)
            {
                return [errors];
            }

            // Enumerables
            if (errors is IEnumerable enumerable && errors is not string)
            {
                var list = new List<object>();

                foreach (var item in enumerable)
                {
                    if (item is not null)
                        list.Add(item);
                }

                return list;
            }

            // Single object
            return [errors];
        }
    }
}
