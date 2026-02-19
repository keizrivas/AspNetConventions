using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Enums;
using AspNetConventions.Http.Models;

namespace AspNetConventions.Core.Abstractions.Models
{
    /// <summary>
    /// Represents a standardized API response with common properties.
    /// </summary>
    /// <remarks>
    /// This abstract base class provides the foundation for all standardized API responses in AspNetConventions,
    /// ensuring consistent response structure across different endpoint types.
    /// </remarks>
    public abstract class ApiResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> class with the specified HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <remarks>
        /// The constructor automatically sets the <see cref="Status"/> property based on the status code.
        /// </remarks>
        protected ApiResponse(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
            Status = StatusCode <= 399 ? ResponseStatus.Success : ResponseStatus.Failure;
        }

        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        [JsonPropertyOrder(1)]
        public ResponseStatus Status { get; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        [JsonPropertyOrder(2)]
        public int StatusCode { get; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets an optional message.
        /// </summary>
        [JsonPropertyOrder(4)]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        [JsonPropertyOrder(6)]
        public Metadata? Metadata { get; set; }

    }
}
