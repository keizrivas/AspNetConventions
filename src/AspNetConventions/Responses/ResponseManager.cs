using System;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Core.Enums;
using AspNetConventions.ExceptionHandling;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AspNetConventions.Responses
{
    internal class ResponseManager(
        AspNetConventionOptions options,
        RequestDescriptor requestDescriptor,
        ILogger? logger = null) : ResponseAdapter(options)
    {

        public ResponseManager(AspNetConventionOptions options, HttpContext httpContext, ILogger? logger = null)
            : this(options, httpContext.GetRequestDescriptor(), logger)
        {
        }

        private readonly ILogger _logger = logger ?? NullLogger<ResponseManager>.Instance;
        private readonly ResponseCollectionResolver _responseCollectionResolver = new(options);
        private readonly IResponseBuilder _responseBuilder = options.Response.GetResponseBuilder(options);
        private readonly IErrorResponseBuilder _errorResponseBuilder = options.Response.GetErrorResponseBuilder(options);

        public override bool IsWrappedResponse(object? data)
        {
            return _responseBuilder.IsWrappedResponse(data)
                || _errorResponseBuilder.IsWrappedResponse(data);
        }

        /// <summary>
        /// Builds a response from HTTP context.
        /// </summary>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseAsync(object? content)
        {
            if (content == null)
            {
                return (null, requestDescriptor.StatusCode);
            }

            var hooks = Options.Response.Hooks;
            var requestResult = GetRequestResultFromContent(content);
            var exceptionDescriptor = content as ExceptionDescriptor;

            // Set metadata
            var metadata = GetMetadata(requestDescriptor, exceptionDescriptor);
            requestResult.SetMetadata(metadata);

            // Determine if we should wrap the response
            var shouldWrap = await hooks.ShouldWrapResponseAsync
                .InvokeAsync<bool?>(requestResult, requestDescriptor)
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
                requestResult.SetPagination(paginationMetadata);
                requestResult.SetData(collection);
            }

            // Invoke hooks before wrapping
            await hooks.BeforeResponseWrapAsync.InvokeAsync(requestResult, requestDescriptor)
                .ConfigureAwait(false);

            // Build the wrapped response
            var wrappedResponse = requestResult.IsSuccess
                ? _responseBuilder.BuildResponse(requestResult, requestDescriptor)
                : _errorResponseBuilder.BuildResponse(requestResult, exceptionDescriptor?.Exception, requestDescriptor);

            // Invoke hooks after wrapping
            await hooks.AfterResponseWrapAsync.InvokeAsync(wrappedResponse, requestResult, requestDescriptor)
                .ConfigureAwait(false);

            return (wrappedResponse, requestDescriptor.StatusCode);
        }

        private RequestResult GetRequestResultFromContent(object? content)
        {
            // Is already a request result
            if (content is RequestResult result)
            {
                return result;
            }

            if (content is ExceptionDescriptor exceptionDescriptor)
            {
                // Check exception envelope status code
                var statusCode = exceptionDescriptor.StatusCode.HasValue
                    ? (HttpStatusCode)exceptionDescriptor.StatusCode.Value
                    : requestDescriptor.StatusCode;

                if (statusCode != requestDescriptor.StatusCode)
                {
                    requestDescriptor.SetStatusCode(statusCode);
                }

                // Parse to response result
                return new RequestResult(
                    data: exceptionDescriptor.Data,
                    type: exceptionDescriptor.Type,
                    message: exceptionDescriptor.Message,
                    statusCode: statusCode);
            }

            // Support ProblemDetails
            if (content is ProblemDetails problemDetails)
            {
                var statusCode = problemDetails.Status.HasValue
                    ? (HttpStatusCode)problemDetails.Status.Value
                    : requestDescriptor.StatusCode;

                if(statusCode != requestDescriptor.StatusCode)
                {
                    requestDescriptor.SetStatusCode(statusCode);
                }

                return new RequestResult(
                    data: problemDetails.Extensions,
                    message: problemDetails.Detail ?? problemDetails.Title,
                    statusCode: statusCode);
            }

            return new RequestResult(
                data: content,
                statusCode: requestDescriptor.StatusCode);
        }

        private Metadata? GetMetadata(RequestDescriptor requestDescriptor, ExceptionDescriptor? exceptionDescriptor)
        {
            if (!Options.Response.IncludeMetadata)
            {
                return null;
            }

            var metadata = (Metadata)requestDescriptor;
            if(exceptionDescriptor == null)
            {
                return metadata;
            }

            // Include stack trace
            if (Options.Response.ErrorResponse.IncludeStackTrace ?? requestDescriptor.IsDevelopment)
            {
                metadata.StackTrace = exceptionDescriptor.Exception?.GetStackTrace();
            }

            // Include exception details
            if (Options.Response.ErrorResponse.IncludeExceptionType ?? requestDescriptor.IsDevelopment)
            {
                metadata.ExceptionType = exceptionDescriptor.Exception?.GetType()?.Name;
            }

            return metadata;
        }

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
                ?? requestDescriptor.HttpContext.GetNumericParameter(pageSizeName)
                ?? Options.Response.Pagination.DefaultPageSize;

            // Determine page number
            var pageNumber = responseCollection.PageNumber
                ?? requestDescriptor.HttpContext.GetNumericParameter(pageNumberName, 1);

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
                    requestDescriptor.HttpContext,
                    caseConverter.Convert(pageSizeName),
                    caseConverter.Convert(pageNumberName)
                );
            }

            return paginationMetadata;
        }
    }
}
