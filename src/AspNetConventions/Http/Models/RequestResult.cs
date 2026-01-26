using System.Net;
using AspNetConventions.Core.Enums;
using AspNetConventions.Extensions;

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
        public void SetData(object? data) => Data = data;

        /// <summary>
        /// Sets the metadata associated with this instance.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        public void SetMetadata(Metadata? metadata) => Metadata = metadata;

        /// <summary>
        /// Sets the pagination associated with this instance.
        /// </summary>
        /// <param name="pagination">The pagination object to associate with this instance.</param>
        public void SetPagination(PaginationMetadata? pagination) => Pagination = pagination;

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
