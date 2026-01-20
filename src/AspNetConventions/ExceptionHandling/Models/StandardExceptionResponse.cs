using System.Net;
using System.Text.Json.Serialization;
using AspNetConventions.Common.Abstractions;

namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Represents a standardized exception response structure.
    /// </summary>
    public sealed class StandardExceptionResponse(HttpStatusCode statusCode) : BaseResponse(statusCode)
    {
        /// <summary>
        /// Gets or sets an type.
        /// </summary>
        [JsonPropertyOrder(-1)]
        public string? Type { get; set; }
    }
}
