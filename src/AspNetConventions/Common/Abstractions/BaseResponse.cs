using System.Net;
using System.Text.Json;
using AspNetConventions.Http;
using AspNetConventions.ResponseFormatting.Enums;

namespace AspNetConventions.Common.Abstractions
{
    public abstract class BaseResponse
    {
        protected BaseResponse(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
            Status = StatusCode <= 299 ? ResponseStatus.Success : ResponseStatus.Failure;
        }

        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        public ResponseStatus Status { get; set; } = ResponseStatus.Success;

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets an optional message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        public Metadata? Metadata { get; set; }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
