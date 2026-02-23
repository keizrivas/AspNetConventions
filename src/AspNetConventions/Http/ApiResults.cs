using System.Collections.Generic;
using System.Net;
using AspNetConventions.Http.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspNetConventions.Http
{
    /// <summary>
    /// Provides factory methods for creating strongly-typed <see cref="ApiResult{TValue}"/> instances with consistent HTTP status codes.
    /// </summary>
    public static partial class ApiResults
    {
        /// <summary>
        /// Produces a <c>200 OK</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the response value.</typeparam>
        /// <param name="value">The value to include in the response body. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the result. Useful for providing additional context to the client.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.OK"/> (200).
        /// </returns>
        public static ApiResult<TValue> Ok<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.OK);

        /// <summary>
        /// Produces a <c>200 OK</c> result carrying only a message, with no typed payload.
        /// </summary>
        /// <param name="message">A message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.OK"/> (200).
        /// </returns>
        public static ApiResult<object?> Ok(string message)
            => new(default, message, HttpStatusCode.OK);

        /// <summary>
        /// Produces a <c>201 Created</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the newly created resource.</typeparam>
        /// <param name="value">The created resource to include in the response body. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.Created"/> (201).
        /// </returns>
        public static ApiResult<TValue> Created<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.Created);

        /// <summary>
        /// Produces a <c>201 Created</c> result carrying only a message, with no typed payload.
        /// </summary>
        /// <param name="message">A message describing the creation result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.Created"/> (201).
        /// </returns>
        public static ApiResult<object?> Created(string message)
            => new(default, message, HttpStatusCode.Created);

        /// <summary>
        /// Produces a <c>202 Accepted</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the response value.</typeparam>
        /// <param name="value">An optional value representing the current state or a tracking reference. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the accepted operation.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.Accepted"/> (202).
        /// </returns>
        public static ApiResult<TValue> Accepted<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.Accepted);

        /// <summary>
        /// Produces a <c>202 Accepted</c> result carrying only a message, with no typed payload.
        /// </summary>
        /// <param name="message">A message describing the accepted operation.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.Accepted"/> (202).
        /// </returns>
        public static ApiResult<object?> Accepted(string message)
            => new(default, message, HttpStatusCode.Accepted);

        /// <summary>
        /// Produces a <c>204 No Content</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.NoContent"/> (204).
        /// </returns>
        public static ApiResult<object?> NoContent(string? message = null)
            => new(default, message, HttpStatusCode.NoContent);

        /// <summary>
        /// Produces a <c>206 Partial Content</c> result with a typed value and optional message.
        /// </summary>
        /// <typeparam name="TValue">The type of the partial response value.</typeparam>
        /// <param name="value">The partial data to include in the response body. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.PartialContent"/> (206).
        /// </returns>
        public static ApiResult<TValue> PartialContent<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.PartialContent);

        /// <summary>
        /// Produces a <c>301 Moved Permanently</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the response value.</typeparam>
        /// <param name="value">An optional value to include in the response. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the redirect.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.MovedPermanently"/> (301).
        /// </returns>
        public static ApiResult<TValue> MovedPermanently<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.MovedPermanently);

        /// <summary>
        /// Produces a <c>302 Found</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the response value.</typeparam>
        /// <param name="value">An optional value to include in the response. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the redirect.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.Found"/> (302).
        /// </returns>
        public static ApiResult<TValue> Found<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.Found);

        /// <summary>
        /// Produces a <c>304 Not Modified</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.NotModified"/> (304).
        /// </returns>
        public static ApiResult<object?> NotModified(string? message = null)
            => new(default, message, HttpStatusCode.NotModified);

        /// <summary>
        /// Produces a <c>400 Bad Request</c> result with validation errors from <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary" /> containing errors to be returned to the client.
        /// Typically populated by model validation.</param>
        /// <param name="message">An optional message describing the validation failure.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <see cref="ValidationProblemDetails"/> with <see cref="System.Net.HttpStatusCode.BadRequest"/> (400).
        /// </returns>
        public static ApiResult<ValidationProblemDetails> BadRequest(ModelStateDictionary modelState, string? message = null)
            => new(new ValidationProblemDetails(modelState), message, HttpStatusCode.BadRequest);

        /// <summary>
        /// Produces a <c>400 Bad Request</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the error detail payload, if any.</typeparam>
        /// <param name="value">An optional object describing the error details. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the error.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.BadRequest"/> (400).
        /// </returns>
        public static ApiResult<TValue> BadRequest<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.BadRequest);

        /// <summary>
        /// Produces a <c>400 Bad Request</c> result with an optional message and no typed payload.
        /// </summary>
        /// <param name="message">An optional message describing the error.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.BadRequest"/> (400).
        /// </returns>
        public static ApiResult<object?> BadRequest(string? message = null)
            => new(default, message, HttpStatusCode.BadRequest);

        /// <summary>
        /// Produces a <c>401 Unauthorized</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the authentication requirement.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.Unauthorized"/> (401).
        /// </returns>
        public static ApiResult<object?> Unauthorized(string? message = null)
            => new(default, message, HttpStatusCode.Unauthorized);

        /// <summary>
        /// Produces a <c>403 Forbidden</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing why access is denied.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.Forbidden"/> (403).
        /// </returns>
        public static ApiResult<object?> Forbidden(string? message = null)
            => new(default, message, HttpStatusCode.Forbidden);

        /// <summary>
        /// Produces a <c>404 Not Found</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing what was not found.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.NotFound"/> (404).
        /// </returns>
        public static ApiResult<object?> NotFound(string? message = null)
            => new(default, message, HttpStatusCode.NotFound);

        /// <summary>
        /// Produces a <c>404 Not Found</c> result with a typed value and optional message.
        /// </summary>
        /// <typeparam name="TValue">The type of the error detail payload.</typeparam>
        /// <param name="value">An optional object describing what was not found. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing what was not found.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.NotFound"/> (404).
        /// </returns>
        public static ApiResult<TValue> NotFound<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.NotFound);

        /// <summary>
        /// Produces a <c>405 Method Not Allowed</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the error.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.MethodNotAllowed"/> (405).
        /// </returns>
        public static ApiResult<object?> MethodNotAllowed(string? message = null)
            => new(default, message, HttpStatusCode.MethodNotAllowed);

        /// <summary>
        /// Produces a <c>408 Request Timeout</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the timeout.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.RequestTimeout"/> (408).
        /// </returns>
        public static ApiResult<object?> RequestTimeout(string? message = null)
            => new(default, message, HttpStatusCode.RequestTimeout);

        /// <summary>
        /// Produces a <c>409 Conflict</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the conflict detail payload.</typeparam>
        /// <param name="value">An optional object describing the conflict details. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the conflict.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.Conflict"/> (409).
        /// </returns>
        public static ApiResult<TValue> Conflict<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.Conflict);

        /// <summary>
        /// Produces a <c>409 Conflict</c> result with an optional message and no typed payload.
        /// </summary>
        /// <param name="message">An optional message describing the conflict.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.Conflict"/> (409).
        /// </returns>
        public static ApiResult<object?> Conflict(string? message = null)
            => new(default, message, HttpStatusCode.Conflict);

        /// <summary>
        /// Produces a <c>410 Gone</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the removal.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.Gone"/> (410).
        /// </returns>
        public static ApiResult<object?> Gone(string? message = null)
            => new(default, message, HttpStatusCode.Gone);

        /// <summary>
        /// Produces a <c>422 Unprocessable Entity</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the validation error detail payload.</typeparam>
        /// <param name="value">An optional object describing the validation errors. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the validation failure.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.UnprocessableEntity"/> (422).
        /// </returns>
        public static ApiResult<TValue> UnprocessableEntity<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.UnprocessableEntity);

        /// <summary>
        /// Produces a <c>422 Unprocessable Entity</c> result with an optional message and no typed payload.
        /// </summary>
        /// <param name="message">An optional message describing the validation failure.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.UnprocessableEntity"/> (422).
        /// </returns>
        public static ApiResult<object?> UnprocessableEntity(string? message = null)
            => new(default, message, HttpStatusCode.UnprocessableEntity);

        /// <summary>
        /// Produces a <c>429 Too Many Requests</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the rate limit.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.TooManyRequests"/> (429).
        /// </returns>
        public static ApiResult<object?> TooManyRequests(string? message = null)
            => new(default, message, HttpStatusCode.TooManyRequests);

        /// <summary>
        /// Produces a <c>500 Internal Server Error</c> result with an optional typed value and message.
        /// </summary>
        /// <typeparam name="TValue">The type of the error detail payload, if any.</typeparam>
        /// <param name="value">An optional object describing the error details. May be <see langword="null"/>.</param>
        /// <param name="message">An optional message describing the server error.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with <see cref="System.Net.HttpStatusCode.InternalServerError"/> (500).
        /// </returns>
        public static ApiResult<TValue> InternalServerError<TValue>(TValue? value, string? message = null)
            => new(value, message, HttpStatusCode.InternalServerError);

        /// <summary>
        /// Produces a <c>500 Internal Server Error</c> result with an optional message and no typed payload.
        /// </summary>
        /// <param name="message">An optional message describing the server error.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.InternalServerError"/> (500).
        /// </returns>
        public static ApiResult<object?> InternalServerError(string? message = null)
            => new(default, message, HttpStatusCode.InternalServerError);

        /// <summary>
        /// Produces a <c>501 Not Implemented</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing what is not implemented.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.NotImplemented"/> (501).
        /// </returns>
        public static ApiResult<object?> NotImplemented(string? message = null)
            => new(default, message, HttpStatusCode.NotImplemented);

        /// <summary>
        /// Produces a <c>502 Bad Gateway</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the gateway error.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.BadGateway"/> (502).
        /// </returns>
        public static ApiResult<object?> BadGateway(string? message = null)
            => new(default, message, HttpStatusCode.BadGateway);

        /// <summary>
        /// Produces a <c>503 Service Unavailable</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the service unavailability.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.ServiceUnavailable"/> (503).
        /// </returns>
        public static ApiResult<object?> ServiceUnavailable(string? message = null)
            => new(default, message, HttpStatusCode.ServiceUnavailable);

        /// <summary>
        /// Produces a <c>504 Gateway Timeout</c> result with an optional message.
        /// </summary>
        /// <param name="message">An optional message describing the timeout.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with <see cref="System.Net.HttpStatusCode.GatewayTimeout"/> (504).
        /// </returns>
        public static ApiResult<object?> GatewayTimeout(string? message = null)
            => new(default, message, HttpStatusCode.GatewayTimeout);

        /// <summary>
        /// Produces a <see cref="ProblemDetails"/> result.
        /// </summary>
        /// <param name="statusCode">The value for <see cref="ProblemDetails.Status" />.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="extensions">The value for <see cref="ProblemDetails.Extensions" />.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <see cref="ProblemDetails"/> for the result.
        /// </returns>
        public static ApiResult<ProblemDetails> Problem(
            string? detail = null,
            string? instance = null,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = detail,
                Instance = instance,
                Status = statusCode,
                Title = title,
                Type = type,
            };

            // Copy extensions
            if (extensions is not null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(extension);
                }
            }

            return new(problemDetails);
        }

        /// <summary>
        /// Produces a <c>200 OK</c> result with a paginated collection containing items and total record count.
        /// </summary>
        /// <typeparam name="TValue">The type of elements contained in the response collection.</typeparam>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="totalRecords">The total number of records available across all pages.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <see cref="CollectionResult{TValue}"/> containing pagination metadata and the current page's items.
        /// </returns>
        public static ApiResult<CollectionResult<TValue>> Paginate<TValue>(IEnumerable<TValue> items, int totalRecords, string? message = null)
            => Ok(new CollectionResult<TValue>(items, totalRecords), message);

        /// <summary>
        /// Produces a <c>200 OK</c> result with a paginated collection containing items, total record count, and pagination parameters.
        /// </summary>
        /// <typeparam name="TValue">The type of elements contained in the response collection.</typeparam>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="totalRecords">The total number of records available across all pages.</param>
        /// <param name="pageNumber">The current page number (1-based index).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <see cref="CollectionResult{TValue}"/> containing pagination metadata and the current page's items.
        /// </returns>
        /// <example>
        public static ApiResult<CollectionResult<TValue>> Paginate<TValue>(IEnumerable<TValue> items, int totalRecords, int pageNumber, int pageSize, string? message = null)
            => Ok(new CollectionResult<TValue>(items, totalRecords, pageNumber, pageSize), message);

        /// <summary>
        /// Produces a result with a custom HTTP status code with a paginated collection containing items, total record count, and pagination parameters.
        /// </summary>
        /// <typeparam name="TValue">The type of elements contained in the response collection.</typeparam>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="totalRecords">The total number of records available across all pages.</param>
        /// <param name="pageNumber">The current page number (1-based index).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> to set on the response.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <see cref="CollectionResult{TValue}"/> containing pagination metadata and the current page's items.
        /// </returns>
        public static ApiResult<CollectionResult<TValue>> Paginate<TValue>(IEnumerable<TValue> items, int totalRecords, int pageNumber, int pageSize, HttpStatusCode statusCode, string? message = null)
            => Custom(new CollectionResult<TValue>(items, totalRecords, pageNumber, pageSize), statusCode, message);

        /// <summary>
        /// Produces a result with a custom HTTP status code, optional typed value, message, and type label.
        /// </summary>
        /// <typeparam name="TValue">The type of the response value.</typeparam>
        /// <param name="value">The value to include in the response body. May be <see langword="null"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> to set on the response.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <param name="type">
        /// An optional custom type label for error categorization.
        /// When <see langword="null"/>, the type is automatically derived from the status code category.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> with the specified status code, value, message, and type.
        /// </returns>
        public static ApiResult<TValue> Custom<TValue>(
            TValue? value,
            HttpStatusCode statusCode,
            string? message = null,
            string? type = null)
            => new(value, message, statusCode, type);

        /// <summary>
        /// Produces a result with a custom HTTP status code, message, and type label.
        /// with no typed payload.
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> to set on the response.</param>
        /// <param name="message">An optional message describing the result.</param>
        /// <param name="type">
        /// An optional custom type label for error categorization.
        /// When <see langword="null"/>, the type is automatically derived from the status code category.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResult{TValue}"/> of <c>object?</c> with the specified status code, message, and type.
        /// </returns>
        public static ApiResult<object?> Custom(
            HttpStatusCode statusCode,
            string? message = null,
            string? type = null)
            => new(default, message, statusCode, type);
    }
}
