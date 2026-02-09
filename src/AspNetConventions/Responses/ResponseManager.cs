using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetConventions.Responses
{
    /// <summary>
    /// Manages the creation and formatting of standardized API responses according to AspNetConventions configuration.
    /// </summary>
    /// <remarks>
    /// This class serves as the central coordinator for response processing, handling various input types and exception descriptors.
    /// </remarks>
    internal class ResponseManager : ResponseAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseManager"/> class.
        /// </summary>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <param name="requestDescriptor">The request descriptor containing context information about the current request.</param>
        /// <param name="logger">Optional logger for diagnostic information. If null, a null logger will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="requestDescriptor"/> is null.</exception>
        public ResponseManager(AspNetConventionOptions options, RequestDescriptor requestDescriptor, ILogger? logger = null)
            : base(options, logger ?? NullLogger<ResponseManager>.Instance)
        {
            ArgumentNullException.ThrowIfNull(requestDescriptor, nameof(requestDescriptor));

            _requestDescriptor = requestDescriptor;
            _responseCollectionResolver = new(options);
            _responseBuilder = options.Response.GetResponseBuilder(options, Logger);
            _errorResponseBuilder = options.Response.GetErrorResponseBuilder(options, Logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseManager"/> class using an HttpContext.
        /// </summary>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="logger">Optional logger for diagnostic information. If null, a null logger will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="httpContext"/> is null.</exception>
        /// <remarks>This constructor automatically extracts request descriptor information from the provided HttpContext.</remarks>
        public ResponseManager(AspNetConventionOptions options, HttpContext httpContext, ILogger? logger = null)
            : this(options, httpContext.GetRequestDescriptor(), logger)
        {
        }

        /// <summary>
        /// The builder used for constructing successful responses.
        /// </summary>
        private readonly IResponseBuilder _responseBuilder;

        /// <summary>
        /// The builder used for constructing error responses.
        /// </summary>
        private readonly IErrorResponseBuilder _errorResponseBuilder;

        /// <summary>
        /// The descriptor containing information about the current HTTP request.
        /// </summary>
        private readonly RequestDescriptor _requestDescriptor;

        /// <summary>
        /// The resolver used to convert data into response collections for pagination support.
        /// </summary>
        private readonly ResponseCollectionResolver _responseCollectionResolver;

        /// <summary>
        /// Determines if the specified data object is already a wrapped response.
        /// </summary>
        /// <param name="data">The data object to check.</param>
        /// <returns>true if the data is already wrapped; otherwise, false.</returns>
        public override bool IsWrappedResponse(object? data)
        {
            return _responseBuilder.IsWrappedResponse(data)
                || _errorResponseBuilder.IsWrappedResponse(data);
        }

        /// <summary>
        /// Determines if the specified request result contains already wrapped response data.
        /// </summary>
        /// <param name="requestResult">The request result to check.</param>
        /// <returns>true if the request result data is already wrapped; otherwise, false.</returns>
        public bool IsWrappedResponse(RequestResult requestResult)
        {
            return IsWrappedResponse(requestResult.Data);
        }

        /// <summary>
        /// Builds a standardized response from the provided content.
        /// </summary>
        /// <param name="content">The content to wrap in a response. Can be null, an object, IResult, RequestResult, or ExceptionDescriptor.</param>
        /// <returns>A tuple containing the wrapped response object and the HTTP status code.</returns>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseAsync(object? content)
        {
            if (content == null)
            {
                return (null, _requestDescriptor.StatusCode);
            }

            var requestResult = GetRequestResultFromContent(content);
            return await BuildResponseAsync(requestResult)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Builds a standardized response from the provided request result.
        /// </summary>
        /// <param name="requestResult">The request result containing response data and metadata.</param>
        /// <returns>A tuple containing the wrapped response object and the HTTP status code.</returns>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseAsync(RequestResult requestResult)
        {
            var hooks = Options.Response.Hooks;
            var exceptionDescriptor = requestResult.Payload as ExceptionDescriptor;

            // Set metadata
            var metadata = GetMetadata(_requestDescriptor, exceptionDescriptor);
            requestResult.WithMetadata(metadata);

            // Determine if we should wrap the response
            var shouldWrap = await hooks.ShouldWrapResponseAsync
                .InvokeAsync<bool?>(requestResult, _requestDescriptor)
                .ConfigureAwait(false) ?? true;

            if (!shouldWrap)
            {
                return (null, requestResult.StatusCode);
            }

            // Handle pagination metadata and resolve collections
            var collection = _responseCollectionResolver.TryResolve(requestResult.Data);
            if (collection is not null)
            {
                var paginationMetadata = GetPaginationMetadata(collection);
                requestResult.WithPagination(paginationMetadata);
                requestResult.WithData(collection);
            }

            // Invoke hooks before wrapping
            await hooks.BeforeResponseWrapAsync.InvokeAsync(requestResult, _requestDescriptor)
                .ConfigureAwait(false);

            // Build the wrapped response
            var wrappedResponse = requestResult.IsSuccess
                ? _responseBuilder.BuildResponse(requestResult, _requestDescriptor)
                : _errorResponseBuilder.BuildResponse(requestResult, exceptionDescriptor?.Exception, _requestDescriptor);

            // Invoke hooks after wrapping
            await hooks.AfterResponseWrapAsync.InvokeAsync(wrappedResponse, requestResult, _requestDescriptor)
                .ConfigureAwait(false);

            return (wrappedResponse, _requestDescriptor.StatusCode);
        }

        /// <summary>
        /// Converts various content types into a standardized <see cref="RequestResult"/>.
        /// </summary>
        /// <param name="content">The content to convert. Can be null, an object, IResult, RequestResult, ExceptionDescriptor, or ProblemDetails.</param>
        /// <returns>A <see cref="RequestResult"/> containing the standardized response data.</returns>
        public RequestResult GetRequestResultFromContent(object? content)
        {
            object? payload = content;
            HttpStatusCode statusCode = _requestDescriptor.StatusCode;

            // Support IValueHttpResult
            if (content is IResult result)
            {
                // Unwrap nested result
                if (result is INestedHttpResult nestedResult)
                {
                    result = nestedResult.Result;
                }

                statusCode =
                    result is IStatusCodeHttpResult { StatusCode: int status }
                        ? (HttpStatusCode)status
                        : _requestDescriptor.StatusCode;

                // Unwrap value
                content = (result as IValueHttpResult)?.Value;
            }

            // Is already a request result
            if (content is RequestResult requestResult)
            {
                return requestResult;
            }

            if (content is ExceptionDescriptor exceptionDescriptor)
            {
                // Check exception envelope status code
                statusCode = exceptionDescriptor.StatusCode ?? _requestDescriptor.StatusCode;

                if (statusCode != _requestDescriptor.StatusCode)
                {
                    _requestDescriptor.SetStatusCode(statusCode);
                }

                // Parse to response result
                return new RequestResult(
                    data: exceptionDescriptor.Data,
                    type: exceptionDescriptor.Type,
                    message: exceptionDescriptor.Message,
                    statusCode: statusCode)
                    .WithPayload(payload);
            }

            // Support ProblemDetails
            if (content is ProblemDetails problemDetails)
            {
                statusCode = problemDetails.Status.HasValue
                    ? (HttpStatusCode)problemDetails.Status.Value
                    : _requestDescriptor.StatusCode;

                if (statusCode != _requestDescriptor.StatusCode)
                {
                    _requestDescriptor.SetStatusCode(statusCode);
                }

                return new RequestResult(
                    data: GetProblemData(problemDetails),
                    message: ResolveMessage(problemDetails),
                    statusCode: statusCode)
                    .WithPayload(payload);
            }

            return new RequestResult(
                data: content,
                statusCode: statusCode)
                .WithPayload(payload);
        }

        /// <summary>
        /// Resolves the most appropriate message to use in the response based on the provided ProblemDetails instance and configuration options.
        /// </summary>
        /// <param name="problem">The ProblemDetails instance from which to resolve the message.</param>
        /// <returns>A string message to be included in the response.</returns>
        private string ResolveMessage(ProblemDetails problem)
        {
            if (!string.IsNullOrWhiteSpace(problem.Detail))
            {
                return problem.Detail;
            }

            if (!string.IsNullOrWhiteSpace(problem.Title))
            {
                return problem.Title;
            }

            return Options.Response.ErrorResponse.DefaultErrorMessage;
        }

        /// <summary>
        /// Extracts relevant data from ProblemDetails extensions based on the allowed keys configured in ErrorResponseOptions.
        /// </summary>
        /// <param name="problem">The ProblemDetails instance from which to extract data.</param>
        /// <returns>An object containing the extracted data, or null if no relevant data is found.</returns>
        private object? GetProblemData(ProblemDetails problem)
        {
            if (problem.Extensions.TryGetValue("errors", out var errors))
            {
                return errors;
            }

            Dictionary<string, object?>? result = null;
            var allowedExtensions = Options.Response.ErrorResponse.AllowedProblemDetailsExtensions;

            // Filter extensions based on allowed list
            foreach (var extension in problem.Extensions)
            {
                if (!allowedExtensions.Contains(extension.Key))
                {
                    continue;
                }

                result ??= [];
                result[extension.Key] = extension.Value;
            }

            return result;
        }

        /// <summary>
        /// Generates metadata for the response based on the request descriptor and exception information.
        /// </summary>
        /// <param name="_requestDescriptor">The request descriptor containing context information.</param>
        /// <param name="exceptionDescriptor">Optional exception descriptor containing exception details.</param>
        /// <returns>A <see cref="Metadata"/> object containing the response metadata, or null if metadata inclusion is disabled.</returns>
        private Metadata? GetMetadata(RequestDescriptor _requestDescriptor, ExceptionDescriptor? exceptionDescriptor)
        {
            if (!Options.Response.IncludeMetadata)
            {
                return null;
            }

            var metadata = (Metadata)_requestDescriptor;
            if (exceptionDescriptor == null)
            {
                return metadata;
            }

            // Include exception details
            if ((Options.Response.ErrorResponse.IncludeExceptionDetails ?? _requestDescriptor.IsDevelopment)
                && exceptionDescriptor.Exception != null)
            {
                metadata.Exception = new ExceptionMetadata(
                    exceptionDescriptor.Exception,
                    Options.Response.ErrorResponse.MaxStackTraceDepth);
            }

            return metadata;
        }

        /// <summary>
        /// Generates pagination metadata for the response based on the provided response collection.
        /// </summary>
        /// <param name="responseCollection">The response collection containing pagination information.</param>
        /// <returns>A <see cref="PaginationMetadata"/> object containing pagination details, or null if pagination metadata inclusion is disabled.</returns>
        private PaginationMetadata? GetPaginationMetadata(IResponseCollection responseCollection)
        {
            ArgumentNullException.ThrowIfNull(responseCollection);

            if (!Options.Response.Pagination.IncludeMetadata)
            {
                return null;
            }

            // Get pagination parameters names
            var pageSizeName = Options.Response.Pagination.PageSizeParameterName;
            var pageNumberName = Options.Response.Pagination.PageNumberParameterName;

            // Determine page size
            var pageSize = responseCollection.PageSize
                ?? _requestDescriptor.HttpContext.GetNumericParameter(pageSizeName)
                ?? Options.Response.Pagination.DefaultPageSize;

            // Determine page number
            var pageNumber = responseCollection.PageNumber
                ?? _requestDescriptor.HttpContext.GetNumericParameter(pageNumberName, 1);

            // Create pagination metadata
            var paginationMetadata = new PaginationMetadata(
                responseCollection.TotalRecords,
                pageNumber,
                pageSize
            );

            // Build pagination links
            if (Options.Response.Pagination.IncludeLinks)
            {
                var caseConverter = Options.Route.GetCaseConverter();
                paginationMetadata.BuildLinks(
                    _requestDescriptor.HttpContext,
                    caseConverter.Convert(pageSizeName),
                    caseConverter.Convert(pageNumberName)
                );
            }

            return paginationMetadata;
        }
    }
}
