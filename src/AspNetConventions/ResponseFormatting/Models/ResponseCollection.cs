using System;
using System.Collections;
using System.Collections.Generic;
using AspNetConventions.ResponseFormatting.Abstractions;

namespace AspNetConventions.ResponseFormatting.Models
{
    /// <summary>
    /// Represents a read-only collection of response items with optional paging and total record count information.
    /// </summary>
    /// <remarks>Use this class to encapsulate a set of items returned from a data source, along with metadata
    /// such as total record count and optional paging details.</remarks>
    /// <typeparam name="T">The type of elements contained in the response collection.</typeparam>
    public sealed class ResponseCollection<T> : IResponseCollection, IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _items;

        public ResponseCollection(IEnumerable<T> items, int totalRecords)
        {
            ArgumentNullException.ThrowIfNull(items);
            _items = items as IReadOnlyList<T> ?? [.. items];
            TotalRecords = totalRecords;
        }

        public ResponseCollection(IEnumerable<T> items, int totalRecords, int pageNumber, int pageSize) : this(items, totalRecords)
        {
            PageSize   = Math.Max(pageSize, 0);
            PageNumber = Math.Max(pageNumber, 0);
        }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int? PageNumber { get; init; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int? PageSize { get; init; }

        /// <summary>
        /// Gets or sets the total number of records.
        /// </summary>
        public int TotalRecords { get; init; }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index] => _items[index];

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Gets an enumerable collection of the items contained in the response.
        /// </summary>
        IEnumerable IResponseCollection.Items => _items;

    }
}
