using System.Net;
using AspNetConventions.Core.Enums;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Encapsulates a standard execution result structure for http request.
    /// </summary>
    public abstract class ApiResult
    {
        // Default response type
        protected const string DefaultResponseType = "SUCCESS";

        // Default status code
        protected const HttpStatusCode DefaultStatusCode = HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; protected set; } = DefaultStatusCode;

        /// <summary>
        /// Gets or sets the response type.
        /// </summary>
        public string Type { get; protected set; } = DefaultResponseType;

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string? Message { get; protected set; }

        /// <summary>
        /// Gets or sets the response payload.
        /// </summary>
        /// <value>The original content before any processing or transformation.</value>
        /// <remarks>This preserves the original content source for audit trails or debugging purposes.</remarks>
        public object? Payload { get; protected set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        /// <value>Metadata about the request context, including timing and trace information.</value>
        public Metadata? Metadata { get; protected set; }

        /// <summary>
        /// Gets or sets pagination metadata.
        /// </summary>
        /// <value>Information about paginated results when applicable.</value>
        public PaginationMetadata? Pagination { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the request was successful.
        /// </summary>
        /// <value>true if the status code is less than 400; otherwise, false.</value>
        /// <remarks>Informational (1xx), Successful (2xx), and Redirection (3xx) responses are considered successful.</remarks>
        public bool IsSuccess => (int)StatusCode <= 399;

        /// <summary>
        /// Retrieves the current result value associated with this instance.
        /// </summary>
        /// <returns>The result value, or null if no data is set.</returns>
        public abstract object? GetValue();

        /// <summary>
        /// Sets the value property and returns the current instance for method chaining.
        /// </summary>
        /// <param name="value">The value object to associate with this instance.</param>
        /// <returns>The current <see cref="ApiResult{TValue}"/> instance with updated value.</returns>
        internal abstract ApiResult WithValue(object? value);

        /// <summary>
        /// Sets the payload property and returns the current instance for method chaining.
        /// </summary>
        /// <param name="data">The payload object to associate with this instance.</param>
        /// <returns>The current <see cref="ApiResult"/> instance with updated payload.</returns>
        internal abstract ApiResult WithPayload(object? data);

        /// <summary>
        /// Sets the metadata associated with this instance and returns the current instance for method chaining.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        /// <returns>The current <see cref="ApiResult"/> instance with updated metadata.</returns>
        internal abstract ApiResult WithMetadata(Metadata? metadata);

        /// <summary>
        /// Sets the pagination associated with this instance and returns the current instance for method chaining.
        /// </summary>
        /// <param name="pagination">The pagination object to associate with this instance.</param>
        /// <returns>The current <see cref="ApiResult"/> instance with updated pagination.</returns>
        internal abstract ApiResult WithPagination(PaginationMetadata? pagination);

        /// <summary>
        /// Creates a new <see cref="ApiResult"/> instance by merging the current instance
        /// with the specified <paramref name="other"/> instance.
        /// </summary>
        /// <param name="other">
        /// The <see cref="ApiResult"/> whose non-null properties will be applied to the current instance.
        /// </param>
        /// <returns>
        /// A new <see cref="ApiResult"/> containing the combined values of the current instance
        /// and the specified <paramref name="other"/> instance.
        /// </returns>
        internal abstract ApiResult Merge(ApiResult other);
    }

    /// <summary>
    /// Encapsulates a standard execution result structure for http request.
    /// </summary>
    /// <typeparam name="TValue">The type of value included in the result.</typeparam>
    public sealed class ApiResult<TValue> : ApiResult
    {
        /// <summary>
        /// Gets or sets the response value.
        /// </summary>
        private TValue? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with value.
        /// </summary>
        /// <param name="value">The value to include in the result.</param>
        public ApiResult(TValue? value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with value and message.
        /// </summary>
        /// <param name="value">The value to include in the result.</param>
        /// <param name="message">The message to include in the result.</param>
        public ApiResult(TValue? value, string? message)
            : this(value)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with value, message, and status code.
        /// </summary>
        /// <param name="value">The value to include in the result.</param>
        /// <param name="message">The message to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        public ApiResult(TValue? value, string? message, HttpStatusCode statusCode)
            : this(value, message)
        {
            StatusCode = statusCode;
            Type = GetResponseType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with value and status code.
        /// </summary>
        /// <param name="value">The value to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        public ApiResult(TValue? value, HttpStatusCode statusCode)
            : this(value, null, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with message and status code.
        /// </summary>
        /// <param name="message">The message to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        public ApiResult(string message, HttpStatusCode statusCode)
            : this(default, message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult{T}"/> class with value, message, status code, and type.
        /// </summary>
        /// <param name="value">The value to include in the result.</param>
        /// <param name="message">The message to include in the result.</param>
        /// <param name="statusCode">The HTTP status code for the result.</param>
        /// <param name="type">The custom type for the result. If null, type is automatically determined.</param>
        public ApiResult(TValue? value, string? message, HttpStatusCode statusCode, string? type)
            : this(value, message, statusCode)
        {
            if (type != null)
            {
                Type = type;
            }
        }

        public override object? GetValue() => _value;

        internal override ApiResult WithValue(object? value)
        {
            _value = (TValue?)value;
            return this;
        }

        internal override ApiResult WithPayload(object? payload)
        {
            Payload = payload;
            return this;
        }

        internal override ApiResult WithMetadata(Metadata? metadata)
        {
            Metadata = metadata;
            return this;
        }

        internal override ApiResult WithPagination(PaginationMetadata? pagination)
        {
            Pagination = pagination;
            return this;
        }

        internal override ApiResult Merge(ApiResult other)
        {
            if (other is null)
            {
                return this;
            }

            var value = other.GetValue();

            // Clone instance with generic object type
            var apiResult = new ApiResult<object>(this.GetValue())
            {
                StatusCode = this.StatusCode,
                Type = this.Type,
                Message = this.Message,
                Payload = this.Payload,
                Metadata = this.Metadata,
                Pagination = this.Pagination
            };

            if (value is not null)
            {
                apiResult = (ApiResult<object>)apiResult.WithValue(value);
            }

            if (!string.IsNullOrWhiteSpace(other.Message))
            {
                apiResult.Message = other.Message;
            }

            if (!string.IsNullOrWhiteSpace(other.Type))
            {
                apiResult.Type = other.Type;
            }

            if (other.Payload is not null)
            {
                apiResult.Payload = other.Payload;
            }

            if (apiResult.StatusCode != other.StatusCode)
            {
                apiResult.StatusCode = other.StatusCode;
            }

            if (other.Metadata is not null)
            {
                apiResult.Metadata = other.Metadata;
            }

            if (other.Pagination is not null)
            {
                apiResult.Pagination = other.Pagination;
            }

            return apiResult;
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

        /// <summary>
        /// Converts an <see cref="ApiResult{TValue}"/> to an <see cref="ActionResult{TValue}"/> implicitly.
        /// </summary>
        /// <param name="result">The <see cref="ApiResult{TValue}"/> to convert.</param>
        /// <returns>An <see cref="ActionResult{TValue}"/> with the appropriate status code.</returns>
        public static implicit operator ActionResult<TValue>(ApiResult<TValue> result)
        {
            return new ObjectResult(result)
            {
                StatusCode = (int)result.StatusCode
            };
        }

        /// <summary>
        /// Converts an <see cref="ApiResult{TValue}"/> to an <see cref="ActionResult"/> implicitly.
        /// </summary>
        /// <param name="result">The <see cref="ApiResult{TValue}"/> to convert.</param>
        /// <returns>An <see cref="ActionResult"/> with the appropriate status code.</returns>
        public static implicit operator ActionResult(ApiResult<TValue> result)
        {
            return new ObjectResult(result)
            {
                StatusCode = (int)result.StatusCode
            };
        }

        /// <summary>
        /// Converts an <see cref="ApiResult{TValue}"/> to a <see cref="HttpApiResult{Tvalue}"/> implicitly.
        /// </summary>
        /// <param name="result">The <see cref="ApiResult{TValue}"/> to convert.</param>
        /// <returns>An <see cref="HttpApiResult{Tvalue}"/>.</returns>
        public static implicit operator HttpApiResult<TValue>(ApiResult<TValue> result)
        {
            return new(result);
        }

        /// <summary>
        /// Converts an <see cref="ApiResult{TValue}"/> to a <see cref="HttpApiResult{Tvalue}"/>.
        /// </summary>
        /// <returns>An <see cref="HttpApiResult{Tvalue}"/>.</returns>
        public HttpApiResult<TValue> ToHttpResult()
        {
            return new(this);
        }
    }
}
