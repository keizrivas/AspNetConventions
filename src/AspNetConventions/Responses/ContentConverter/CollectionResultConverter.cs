using System;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Extensions;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Responses.ContentConverter
{
    /// <summary>
    /// Converts collection results to standardized API responses with pagination metadata.
    /// </summary>
    /// <remarks>
    /// This converter handles <see cref="ICollectionResult"/> implementations and supports
    /// custom adapters for converting external collection types into the standard format.
    /// </remarks>
    internal class CollectionResultConverter(AspNetConventionOptions options) : IApiResultConverter
    {
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        private ICollectionResult? _collectionResult;

        public bool CanConvert(object content)
        {
            _collectionResult = TryResolve(content);
            return _collectionResult != null;
        }

        public ApiResult Convert(object content, RequestDescriptor requestDescriptor)
        {
            var paginationMetadata = GetPaginationMetadata(_collectionResult!, requestDescriptor);

            return new ApiResult<object>(
                value: _collectionResult!.Items,
                statusCode: requestDescriptor.StatusCode)
                .WithPagination(paginationMetadata);
        }

        /// <summary>
        /// Attempts to resolve the provided data into an <see cref="ICollectionResult"/> using registered adapters.
        /// </summary>
        /// <param name="data">The data to resolve.</param>
        /// <returns>The resolved <see cref="ICollectionResult"/> if successful; otherwise, null.</returns>
        private ICollectionResult? TryResolve(object data)
        {
            // Already our standard type
            if (data is ICollectionResult collection)
            {
                return collection;
            }

            // Try adapters
            foreach (var adapter in _options.Response.CollectionResultAdapters)
            {
                if (adapter.CanHandle(data))
                {
                    return adapter.Convert(data);
                }
            }

            return null;
        }

        /// <summary>
        /// Generates pagination metadata for the response based on the provided response collection.
        /// </summary>
        /// <param name="collectionResult">The collection result containing pagination information.</param>
        /// <returns>A <see cref="PaginationMetadata"/> object containing pagination details, or null if pagination metadata inclusion is disabled.</returns>
        private PaginationMetadata? GetPaginationMetadata(ICollectionResult collectionResult, RequestDescriptor requestDescriptor)
        {
            if (!_options.Response.Pagination.IncludeMetadata)
            {
                return null;
            }

            // Get pagination parameters names
            var pageSizeName = _options.Response.Pagination.PageSizeParameterName;
            var pageNumberName = _options.Response.Pagination.PageNumberParameterName;

            // Determine page size
            var pageSize = collectionResult.PageSize
                ?? requestDescriptor.HttpContext.GetNumericParameter(pageSizeName)
                ?? _options.Response.Pagination.DefaultPageSize;

            // Determine page number
            var pageNumber = collectionResult.PageNumber
                ?? requestDescriptor.HttpContext.GetNumericParameter(pageNumberName, 1);

            // Create pagination metadata
            var paginationMetadata = new PaginationMetadata(
                collectionResult.TotalRecords,
                pageNumber,
                pageSize
            );

            // Build pagination links
            if (_options.Response.Pagination.IncludeLinks)
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
