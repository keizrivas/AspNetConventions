using System.Net;
using AspNetConventions.Core.Enums;
using AspNetConventions.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Encapsulates a standard execution result structure for http request.
    /// </summary>
    public sealed class RequestResult
    {
        public RequestResult()
        { }

        public RequestResult(object? data)
        {
            Data = data;
        }

        public RequestResult(object? data, string? message)
            : this(data)
        {
            Message = message;
        }

        public RequestResult(object? data, string? message, HttpStatusCode statusCode)
            : this(data, message)
        {
            StatusCode = statusCode;
            Type = GetResponseType();
        }

        public RequestResult(object? data, HttpStatusCode statusCode)
            : this(data, null, statusCode)
        {
        }

        public RequestResult(object? data, string? message, HttpStatusCode statusCode, string? type)
            : this(data, message, statusCode)
        {
            if(type != null)
            {
                Type = type;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets the response type.
        /// </summary>
        public string Type { get; init; } = "SUCCESS";

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public object? Data { get; private set; }

        /// <summary>
        /// Gets or sets the response payload.
        /// </summary>
        public object? Payload { get; private set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        public Metadata? Metadata { get; private set; }

        /// <summary>
        /// Gets or sets pagination metadata.
        /// </summary>
        public PaginationMetadata? Pagination { get; private set; }

        public bool IsSuccess => (int)StatusCode <= 399;

        /// <summary>
        /// Sets the data property.
        /// <param name="data">The data object to associate with this instance.</param>
        /// </summary>
        public RequestResult WithData(object? data)
        {
            Data = data;
            return this;
        }

        /// <summary>
        /// Sets the payload property.
        /// <param name="data">The payload object to associate with this instance.</param>
        /// </summary>
        public RequestResult WithPayload(object? data)
        {
            Payload = data;
            return this;
        }

        /// <summary>
        /// Sets the metadata associated with this instance.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        public RequestResult WithMetadata(Metadata? metadata)
        {
            Metadata = metadata;
            return this;
        }

        /// <summary>
        /// Sets the pagination associated with this instance.
        /// </summary>
        /// <param name="pagination">The pagination object to associate with this instance.</param>
        public RequestResult WithPagination(PaginationMetadata? pagination)
        {
            Pagination = pagination;
            return this;
        }

        private string GetResponseType()
        {
            var statusCodeType = StatusCode.GetHttpStatusCodeType();
            return statusCodeType switch
            {
                HttpStatusCodeType.Informational => "INFORMATIONAL",
                HttpStatusCodeType.Success => "SUCCESS",
                HttpStatusCodeType.Redirection => "REDIRECTION",
                HttpStatusCodeType.ClientError => "CLIENT_ERROR",
                HttpStatusCodeType.ServerError => "SERVER_ERROR",
                _ => "UNKNOWN"
            };
        }
    }
}
