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
using AspNetConventions.Responses.ContentConverter;
using Microsoft.AspNetCore.Http;
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
    internal sealed class ResponseFactory : ResponseAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseFactory"/> class.
        /// </summary>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <param name="requestDescriptor">The request descriptor containing context information about the current request.</param>
        /// <param name="logger">Optional logger for diagnostic information. If null, a null logger will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="requestDescriptor"/> is null.</exception>
        public ResponseFactory(AspNetConventionOptions options, RequestDescriptor requestDescriptor, ILogger? logger = null)
            : base(options, logger ?? NullLogger<ResponseFactory>.Instance)
        {
            ArgumentNullException.ThrowIfNull(requestDescriptor, nameof(requestDescriptor));

            _requestDescriptor = requestDescriptor;
            _responseBuilder = options.Response.GetResponseBuilder(options, Logger);
            _errorResponseBuilder = options.Response.GetErrorResponseBuilder(options, Logger);
            _converters =
            [
                new ExceptionDescriptorConverter(),
                new ModelStateDictionaryConverter(options),
                new ProblemDetailsConverter(options),
                new CollectionResultConverter(options),
            ];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseFactory"/> class using an HttpContext.
        /// </summary>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="logger">Optional logger for diagnostic information. If null, a null logger will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="httpContext"/> is null.</exception>
        /// <remarks>This constructor automatically extracts request descriptor information from the provided HttpContext.</remarks>
        public ResponseFactory(AspNetConventionOptions options, HttpContext httpContext, ILogger? logger = null)
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
        /// Provides a read-only collection of converters used to transform content results into standardized formats.
        /// </summary>
        private readonly IReadOnlyList<IApiResultConverter> _converters;

        /// <summary>
        /// The descriptor containing information about the current HTTP request.
        /// </summary>
        private readonly RequestDescriptor _requestDescriptor;

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
        /// <param name="apiResult">The request result to check.</param>
        /// <returns>true if the request result data is already wrapped; otherwise, false.</returns>
        public bool IsWrappedResponse(ApiResult apiResult)
        {
            return IsWrappedResponse(apiResult.GetValue());
        }

        /// <summary>
        /// Builds a standardized response from the provided content.
        /// </summary>
        /// <param name="content">The content to wrap in a response. Can be null, an object, IResult, ApiResult, or ExceptionDescriptor.</param>
        /// <returns>A tuple containing the wrapped response object and the HTTP status code.</returns>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseAsync(object? content)
        {
            if (content == null)
            {
                return (null, _requestDescriptor.StatusCode);
            }

            var apiResult = GetRequestResultFromContent(content);
            return await BuildResponseAsync(apiResult)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Builds a standardized response from the provided request result.
        /// </summary>
        /// <param name="apiResult">The request result containing response data and metadata.</param>
        /// <returns>A tuple containing the wrapped response object and the HTTP status code.</returns>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseAsync(ApiResult apiResult)
        {
            var hooks = Options.Response.Hooks;
            var exceptionDescriptor = apiResult.Payload as ExceptionDescriptor;

            // Set metadata
            var metadata = GetMetadata(_requestDescriptor, exceptionDescriptor);
            apiResult.WithMetadata(metadata);

            // Determine if we should wrap the response
            var shouldWrap = await hooks.ShouldWrapResponseAsync
                .InvokeAsync<bool?>(apiResult, _requestDescriptor)
                .ConfigureAwait(false) ?? true;

            if (!shouldWrap)
            {
                return (null, apiResult.StatusCode);
            }

            // Invoke hooks before wrapping
            await hooks.BeforeResponseWrapAsync.InvokeAsync(apiResult, _requestDescriptor)
                .ConfigureAwait(false);

            // Build the wrapped response
            var wrappedResponse = apiResult.IsSuccess
                ? _responseBuilder.BuildResponse(apiResult, _requestDescriptor)
                : _errorResponseBuilder.BuildResponse(apiResult, exceptionDescriptor?.Exception, _requestDescriptor);

            // Invoke hooks after wrapping
            await hooks.AfterResponseWrapAsync.InvokeAsync(wrappedResponse, apiResult, _requestDescriptor)
                .ConfigureAwait(false);

            return (wrappedResponse, _requestDescriptor.StatusCode);
        }

        /// <summary>
        /// Converts various content types into a standardized <see cref="ApiResult"/>.
        /// </summary>
        /// <param name="content">The content to convert. Can be null.</param>
        /// <returns>A <see cref="ApiResult"/> containing the standardized response data.</returns>
        public ApiResult GetRequestResultFromContent(object? content)
        {
            object? payload = content;
            var statusCode = _requestDescriptor.StatusCode;

            // Handle null content
            if (content is null)
            {
                return new ApiResult<object>(
                    value: null,
                    statusCode: statusCode);
            }

            // Is already a api result
            if (content is ApiResult apiResult)
            {
                var value = apiResult.GetValue();

                // Check for unprocessed content that can be converted to the expected value type
                var normalized = GetRequestResultFromContent(value);

                // Return the original api result if the normalized value is the same as the original value
                if (ReferenceEquals(normalized.GetValue(), value))
                {
                    return apiResult
                        .WithPayload(payload);
                }

                // Set new payload
                payload = value;

                return apiResult
                    .Merge(normalized)
                    .WithPayload(payload);
            }

            // Check converters for supported content types
            foreach (var converter in _converters)
            {
                if (converter.CanConvert(content))
                {
                    return converter
                        .Convert(content, _requestDescriptor)
                        .WithPayload(payload);
                }
            }

            // Default fallback
            return new ApiResult<object>(
                value: content,
                statusCode: statusCode)
                .WithPayload(payload);
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
                if (Options.Response.ErrorResponse.IncludeExceptionDetails == true && !_requestDescriptor.IsDevelopment)
                {
                    Logger.LogDisclosureVulnerabilityWarning("Exception details should not be exposed in non-development environments.");
                }

                metadata.Exception = new ExceptionMetadata(
                    exceptionDescriptor.Exception,
                    Options.Response.ErrorResponse.MaxStackTraceDepth);
            }

            return metadata;
        }
    }
}
