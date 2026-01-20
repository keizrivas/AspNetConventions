using System;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Configuration;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Http;
using AspNetConventions.ResponseFormatting.Abstractions;
using AspNetConventions.ResponseFormatting.Models;
using Microsoft.AspNetCore.Http;

namespace AspNetConventions.ResponseFormatting
{
    /// <summary>
    /// Provides helper methods for response handling.
    /// </summary>
    internal sealed class ResponseHelpers(IResponseCollectionResolver collectionResolver, AspNetConventionOptions options, RequestDescriptor requestDescriptor) : ResponseAdapter(options)
    {
        private readonly AspNetConventionOptions _options = options;
        private readonly IResponseBuilder _builder = options.Response.GetResponseBuilder(options);

        public ResponseHelpers(IResponseCollectionResolver collectionResolver, AspNetConventionOptions options, HttpContext httpContext)
            : this(collectionResolver, options, httpContext.ToRequestDescriptor())
        {
        }

        public override bool IsWrappedResponse(object? data)
        {
            return _builder.IsWrappedResponse(data);
        }

        public bool ShouldHandleResponse(object? data = null)
        {
            return _options.Response.IsEnabled && !IsWrappedResponse(data);
        }

        /// <summary>
        /// Builds a response from HTTP context.
        /// </summary>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseAsync(object? data)
        {
            var hooks = _options.Response.Hooks;

            IResponseEnvelope? baseEnvelope = null;
            ResponseEnvelope? responseEnvelope = null;

            // Check if data is already an envelope
            if (data is IResponseEnvelope envelope)
            {
                baseEnvelope = envelope;

                // If data is an exception envelope set status code
                if (baseEnvelope is ExceptionEnvelope exceptionEnvelope)
                {
                    // Check exception envelope status code
                    if (exceptionEnvelope.StatusCode == default)
                    {
                        exceptionEnvelope.StatusCode = requestDescriptor.StatusCode;
                    }
                    else
                    {
                        requestDescriptor.SetStatusCode(exceptionEnvelope.StatusCode);
                    }

                    // Convert to response envelope
                    responseEnvelope = (ResponseEnvelope)exceptionEnvelope;
                }
                else
                {
                    responseEnvelope = baseEnvelope as ResponseEnvelope;
                }
            }

            // Prepare response envelope if available
            responseEnvelope ??= new ResponseEnvelope(
                data: data,
                statusCode: requestDescriptor.StatusCode
            );

            // Assign base envelope if available
            baseEnvelope ??= responseEnvelope;

            // Determine if we should wrap the response
            var shouldWrap = await hooks.ShouldWrapResponseAsync.InvokeAsync<bool?>(baseEnvelope, requestDescriptor)
                .ConfigureAwait(false) ?? true;

            if (!shouldWrap)
            {
                // If not wrapping and it's ExceptionEnvelope type, return message or data
                data = baseEnvelope is ExceptionEnvelope exceptionEnvelope
                    ? exceptionEnvelope.Message ?? exceptionEnvelope.Data
                    : data;

                return (data, requestDescriptor.StatusCode);
            }

            // Set metadata
            if (_options.Response.IncludeMetadata)
            {
                responseEnvelope.SetMetadata((Metadata)requestDescriptor);
            }

            // Extract data
            var dataObject = responseEnvelope.Data;

            // Handle pagination metadata and resolve collections
            var paginationMetadata = GetPaginationMetadata(ref dataObject);
            responseEnvelope.SetPagination(paginationMetadata);
            responseEnvelope.SetData(dataObject);

            // Invoke hooks before wrapping
            await hooks.BeforeResponseWrapAsync.InvokeAsync(baseEnvelope, requestDescriptor)
                .ConfigureAwait(false);

            // Build the wrapped response
            var wrappedResponse = _builder.BuildResponse(responseEnvelope, requestDescriptor);

            // Invoke hooks after wrapping
            await hooks.AfterResponseWrapAsync.InvokeAsync(wrappedResponse, baseEnvelope, requestDescriptor)
                .ConfigureAwait(false);

            return (wrappedResponse, requestDescriptor.StatusCode);
        }

        private PaginationMetadata? GetPaginationMetadata(ref object? data)
        {
            if (data == null)
            {
                return null;
            }

            // Handle IResponseCollection directly
            if (data is IResponseCollection responseCollection)
            {
                return CreatePaginationMetadata(responseCollection);
            }

            // Handle response collections
            var collection = collectionResolver.TryResolve(data);
            if (collection is not null)
            {
                data = collection;
                return CreatePaginationMetadata(collection);
            }

            return null;
        }

        private PaginationMetadata? CreatePaginationMetadata(IResponseCollection responseCollection)
        {
            ArgumentNullException.ThrowIfNull(requestDescriptor);
            ArgumentNullException.ThrowIfNull(responseCollection);

            if (!_options.Response.IncludePaginationMetadata)
            {
                return null;
            }

            // Get pagination parameters names
            var pageSizeName = _options.Response.PageSizeQueryParameterName;
            var pageNumberName = _options.Response.PageNumberQueryParameterName;

            // Determine page size
            var pageSize = responseCollection.PageSize
                ?? requestDescriptor.HttpContext.GetNumericParameter(pageSizeName)
                ?? _options.Response.DefaultPageSize;

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
            if (_options.Response.IncludePaginationLinks)
            {
                var caseConverter = _options.Route.GetCaseConverter();
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
