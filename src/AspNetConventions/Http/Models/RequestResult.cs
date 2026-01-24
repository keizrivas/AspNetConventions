using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Encapsulates a standard execution result structure for http request.
    /// </summary>
    public sealed class RequestResult
    {
        public RequestResult() { }

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
        }

        public RequestResult(object? data, HttpStatusCode statusCode)
            : this(data, null, statusCode)
        {
        }

        public RequestResult(object? data, string? message, HttpStatusCode statusCode, string? type)
            : this(data, message, statusCode)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets the response type.
        /// </summary>
        public string? Type { get; private set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string? Message { get; private set; }

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

        public bool IsSuccess => (int)StatusCode <= 299;

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

    }
}
