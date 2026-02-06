using System;
using System.Collections.Generic;
using System.Globalization;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Contains metadata about paginated results, including pagination information and navigation links.
    /// </summary>
    /// <remarks>
    /// This class provides comprehensive pagination information for API responses, including current page details,
    /// total counts, and navigation links to first, last, next, and previous pages when link generation is enabled.
    /// </remarks>
    /// <param name="totalRecords">The total number of records available across all pages.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    public sealed class PaginationMetadata(int totalRecords, int pageNumber, int pageSize)
    {
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        /// <value>The page number, automatically normalized to be at least 1.</value>
        public int PageNumber { get; set; } = Math.Max(pageNumber, 1);

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        /// <value>The number of items to display per page.</value>
        public int PageSize { get; set; } = pageSize;

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        /// <value>Automatically calculated based on total records and page size.</value>
        public int TotalPages { get; set; } = Math.Max((int)Math.Ceiling(totalRecords / (double)pageSize), 0);

        /// <summary>
        /// Gets or sets the total number of records available across all pages.
        /// </summary>
        /// <value>The total count of items in the data source.</value>
        public int TotalRecords { get; set; } = totalRecords;

        /// <summary>
        /// Gets or sets pagination navigation links.
        /// </summary>
        /// <value>Contains links to first, last, next, and previous pages when link generation is enabled.</value>
        public PaginationLinks? Links { get; set; }

        /// <summary>
        /// Builds pagination navigation links based on the current HTTP request context.
        /// </summary>
        /// <param name="context">The HTTP context containing request information.</param>
        /// <param name="pageSizeName">The query parameter name for page size (e.g., "pageSize").</param>
        /// <param name="pageNumberName">The query parameter name for page number (e.g., "pageNumber").</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pageSizeName"/> or <paramref name="pageNumberName"/> is null or empty.</exception>
        /// <remarks>
        /// This method constructs URLs for first, last, next, and previous pages while preserving
        /// other query parameters from the original request.
        /// </remarks>
        public void BuildLinks(HttpContext context, string pageSizeName, string pageNumberName)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentException.ThrowIfNullOrWhiteSpace(pageSizeName, nameof(pageSizeName));
            ArgumentException.ThrowIfNullOrWhiteSpace(pageNumberName, nameof(pageNumberName));

            var request = context.Request;
            var baseUri = $"{request.Scheme}://{request.Host.ToUriComponent()}{request.Path}";
            var paramList = new Dictionary<string, string>();

            // No-pagination query parameters
            foreach (var param in request.Query)
            {
                var normalizedKey = param.Key.NormalizeQueryKey();
                if (normalizedKey != pageNumberName.NormalizeQueryKey() &&
                    normalizedKey != pageSizeName.NormalizeQueryKey())
                {
                    paramList.Add(param.Key, param.Value.ToString());
                }
            }

            Links = new PaginationLinks
            {
                FirstPageUrl = BuildPageUri(baseUri, paramList, 1, pageSizeName, pageNumberName),
                LastPageUrl = BuildPageUri(baseUri, paramList, TotalPages, pageSizeName, pageNumberName),
                NextPageUrl = PageNumber < TotalPages
                        ? BuildPageUri(baseUri, paramList, PageNumber + 1, pageSizeName, pageNumberName)
                        : null,
                PreviousPageUrl = PageNumber > 1
                        ? BuildPageUri(baseUri, paramList, PageNumber - 1, pageSizeName, pageNumberName)
                        : null
            };
        }

        /// <summary>
        /// Builds a URI for a specific page number while preserving existing query parameters.
        /// </summary>
        /// <param name="baseUri">The base URI without query parameters.</param>
        /// <param name="paramList">Dictionary of existing query parameters to preserve.</param>
        /// <param name="pageNumber">The page number to build the URI for.</param>
        /// <param name="pageSizeName">The query parameter name for page size.</param>
        /// <param name="pageNumberName">The query parameter name for page number.</param>
        /// <returns>A complete URI with the specified page parameters and preserved query parameters.</returns>
        private Uri BuildPageUri(
            string baseUri,
            Dictionary<string, string> paramList,
            int pageNumber,
            string pageSizeName,
            string pageNumberName)
        {
            var uri = QueryHelpers.AddQueryString(baseUri, pageNumberName, pageNumber.ToString(CultureInfo.InvariantCulture));
            uri = QueryHelpers.AddQueryString(uri, pageSizeName, PageSize.ToString(CultureInfo.InvariantCulture));

            // Re-add other query parameters
            foreach (var param in paramList)
            {
                uri = QueryHelpers.AddQueryString(uri, param.Key, param.Value.ToString());
            }

            return new Uri(uri);
        }
    }
}
