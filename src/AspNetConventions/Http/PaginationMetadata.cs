using System;
using System.Collections.Generic;
using System.Globalization;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace AspNetConventions.Http
{
    /// <summary>
    /// Metadata about paginated results.
    /// </summary>
    public sealed class PaginationMetadata(int totalRecords, int pageNumber, int pageSize)
    {
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; } = pageNumber;

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; } = pageSize;

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; } =  Math.Max((int)Math.Ceiling(totalRecords / (double)pageSize), 0);

        /// <summary>
        /// Gets or sets the total number of records.
        /// </summary>
        public int TotalRecords { get; set; } = totalRecords;

        /// <summary>
        /// Gets or sets pagination links.
        /// </summary>
        public PaginationLinks? Links { get; set; }

        public void BuildLinks(HttpContext context, string pageSizeName, string pageNumberName)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

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
                LastPageUrl  = BuildPageUri(baseUri, paramList, TotalPages, pageSizeName, pageNumberName),
                NextPageUrl  = PageNumber < TotalPages
                        ? BuildPageUri(baseUri, paramList, PageNumber + 1, pageSizeName, pageNumberName)
                        : null,
                PreviousPageUrl = PageNumber > 1
                        ? BuildPageUri(baseUri, paramList, PageNumber - 1, pageSizeName, pageNumberName)
                        : null
            };
        }

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
