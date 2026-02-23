using System;
using System.Collections;
using System.Collections.Generic;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Represents a read-only collection of response items with optional paging and total record count information.
    /// </summary>
    /// <remarks>Use this class to encapsulate a set of items returned from a data source, along with metadata
    /// such as total record count and optional paging details.</remarks>
    /// <typeparam name="T">The type of elements contained in the response collection.</typeparam>
    public sealed class CollectionResult<T> : ICollectionResult, IReadOnlyList<T>
    {
        /// <summary>
        /// The underlying read-only list of items in the collection.
        /// </summary>
        private readonly IReadOnlyList<T> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionResult{T}"/> class with total records count.
        /// </summary>
        /// <param name="items">The collection of items to include in the response.</param>
        /// <param name="totalRecords">The total number of records available across all pages.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public CollectionResult(IEnumerable<T> items, int totalRecords)
        {
            ArgumentNullException.ThrowIfNull(items);
            _items = items as IReadOnlyList<T> ?? [.. items];
            TotalRecords = totalRecords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionResult{T}"/> class with pagination information.
        /// </summary>
        /// <param name="items">The collection of items to include in the current page.</param>
        /// <param name="totalRecords">The total number of records available across all pages.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public CollectionResult(IEnumerable<T> items, int totalRecords, int pageNumber, int pageSize) : this(items, totalRecords)
        {
            PageSize = Math.Max(pageSize, 0);
            PageNumber = Math.Max(pageNumber, 1);
        }

        /// <summary>
        /// Gets the current page number in a paginated response.
        /// </summary>
        /// <value>The 1-based page number, or null if the response is not paginated.</value>
        public int? PageNumber { get; init; }

        /// <summary>
        /// Gets the page size used in a paginated response.
        /// </summary>
        /// <value>The number of items per page, or null if the response is not paginated.</value>
        public int? PageSize { get; init; }

        /// <summary>
        /// Gets the total number of records available across all pages.
        /// </summary>
        /// <value>The total count of items in the data source, regardless of pagination.</value>
        /// <remarks>
        /// This value represents the complete dataset size, which may be larger than
        /// the current page's item count when pagination is used.
        /// </remarks>
        public int TotalRecords { get; init; }

        /// <summary>
        /// Gets the number of elements contained in the current page of the collection.
        /// </summary>
        /// <value>The count of items in the current page, which may be less than <see cref="TotalRecords"/> when paginated.</value>
        public int Count => _items.Count;

        /// <summary>
        /// Gets the element at the specified index in the current page.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve within the current page.</param>
        /// <returns>The element at the specified index in the current page.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is outside the bounds of the current page.</exception>
        public T this[int index] => _items[index];

        /// <summary>
        /// Returns an enumerator that iterates through the items in the current page.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the current page's items.</returns>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the items in the current page.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the current page's items.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Gets an enumerable collection of the items contained in the current page of the response.
        /// </summary>
        /// <value>An enumerable view of the current page's items for the <see cref="ICollectionResult"/> interface.</value>
        IEnumerable ICollectionResult.Items => _items;

    }
}
