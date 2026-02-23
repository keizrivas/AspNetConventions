using System.Collections;

namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Represents a read-only collection of response items with optional pagination metadata.
    /// </summary>
    /// <remarks>Implementations of this interface provide access to a set of items, along with information
    /// about the total number of records and optional paging details. This interface is typically used to represent
    /// paginated results from data queries or service responses.</remarks>
    public interface ICollectionResult
    {
        /// <summary>
        /// Items contained in the collection.
        /// </summary>
        IEnumerable Items { get; }

        /// <summary>
        /// Gets the total number of records available in the data source.
        /// </summary>
        int TotalRecords { get; }

        /// <summary>
        /// Gets the current page number within the paginated result set.
        /// </summary>
        int? PageNumber { get; }

        /// <summary>
        /// Gets the current page size within the paginated result set.
        /// </summary>
        int? PageSize { get; }
    }
}
