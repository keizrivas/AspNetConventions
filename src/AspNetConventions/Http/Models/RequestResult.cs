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
        private const string DefaultResponseType = "SUCCESS";

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult"/> class with default values.
        /// </summary>
        /// <remarks>Creates a successful result with no data and default OK status code.</remarks>
        public RequestResult()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult"/> class with data.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        /// <remarks>Creates a successful result with the provided data and default OK status code.</remarks>
        public RequestResult(object? data)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult"/> class with data and message.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        /// <param name="message">The message to include in the result.</param>
        /// <remarks>Creates a successful result with the provided data and message, using default OK status code.</remarks>
        public RequestResult(object? data, string? message)
            : this(data)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult"/> class with data, message, and status code.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        /// <param name="message">The message to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        /// <remarks>The type is automatically determined based on the status code.</remarks>
        public RequestResult(object? data, string? message, HttpStatusCode statusCode)
            : this(data, message)
        {
            StatusCode = statusCode;
            Type = GetResponseType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult"/> class with data and status code.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        /// <remarks>Creates a result with the provided data and status code. The type is automatically determined.</remarks>
        public RequestResult(object? data, HttpStatusCode statusCode)
            : this(data, null, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult"/> class with data, message, status code, and type.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        /// <param name="message">The message to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        /// <param name="type">The custom type for the result. If null, type is automatically determined.</param>
        public RequestResult(object? data, string? message, HttpStatusCode statusCode, string? type)
            : this(data, message, statusCode)
        {
            if (type != null)
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
        public string Type { get; init; } = DefaultResponseType;

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
        /// <value>The original content before any processing or transformation.</value>
        /// <remarks>This preserves the original content source for audit trails or debugging purposes.</remarks>
        public object? Payload { get; private set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        /// <value>Metadata about the request context, including timing and trace information.</value>
        public Metadata? Metadata { get; private set; }

        /// <summary>
        /// Gets or sets pagination metadata.
        /// </summary>
        /// <value>Information about paginated results when applicable.</value>
        public PaginationMetadata? Pagination { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request was successful.
        /// </summary>
        /// <value>true if the status code is less than 400; otherwise, false.</value>
        /// <remarks>Informational (1xx), Successful (2xx), and Redirection (3xx) responses are considered successful.</remarks>
        public bool IsSuccess => (int)StatusCode <= 399;

        /// <summary>
        /// Sets the data property and returns the current instance for method chaining.
        /// </summary>
        /// <param name="data">The data object to associate with this instance.</param>
        /// <returns>The current <see cref="RequestResult"/> instance with updated data.</returns>
        public RequestResult WithData(object? data)
        {
            Data = data;
            return this;
        }

        /// <summary>
        /// Sets the payload property and returns the current instance for method chaining.
        /// </summary>
        /// <param name="data">The payload object to associate with this instance.</param>
        /// <returns>The current <see cref="RequestResult"/> instance with updated payload.</returns>
        public RequestResult WithPayload(object? data)
        {
            Payload = data;
            return this;
        }

        /// <summary>
        /// Sets the metadata associated with this instance and returns the current instance for method chaining.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        /// <returns>The current <see cref="RequestResult"/> instance with updated metadata.</returns>
        public RequestResult WithMetadata(Metadata? metadata)
        {
            Metadata = metadata;
            return this;
        }

        /// <summary>
        /// Sets the pagination associated with this instance and returns the current instance for method chaining.
        /// </summary>
        /// <param name="pagination">The pagination object to associate with this instance.</param>
        /// <returns>The current <see cref="RequestResult"/> instance with updated pagination.</returns>
        public RequestResult WithPagination(PaginationMetadata? pagination)
        {
            Pagination = pagination;
            return this;
        }

        /// <summary>
        /// Determines the response type based on the HTTP status code.
        /// </summary>
        /// <returns>A string representing the response type category.</returns>
        /// <remarks>
        /// This method uses the <see cref="HttpStatusCodeExtensions.GetHttpStatusCodeType"/> extension method
        /// to categorize the status code and returns a corresponding string representation.
        /// </remarks>
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
                _ => DefaultResponseType
            };
        }
    }
}
