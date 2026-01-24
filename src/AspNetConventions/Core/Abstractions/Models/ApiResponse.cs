using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Core.Enums;
using AspNetConventions.Http.Models;

namespace AspNetConventions.Core.Abstractions.Models
{
    public abstract class ApiResponse
    {
        protected ApiResponse(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
            Status = StatusCode <= 299 ? ResponseStatus.Success : ResponseStatus.Failure;
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
        public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

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
