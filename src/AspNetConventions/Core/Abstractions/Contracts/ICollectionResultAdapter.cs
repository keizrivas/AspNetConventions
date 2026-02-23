namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for adapting data objects into an <see cref="ICollectionResult"/>.
    /// </summary>
    /// <remarks>Implementations of this interface enable flexible conversion of various data formats or types
    /// into a standardized <see cref="ICollectionResult"/>.</remarks>
    public interface ICollectionResultAdapter
    {
        /// <summary>
        /// Returns true if this adapter can convert the given data object into an <see cref="ICollectionResult"/>.
        /// </summary>
        bool CanHandle(object data);

        /// <summary>
        /// Converts the given data object into an <see cref="ICollectionResult"/>.
        /// </summary>
        ICollectionResult Convert(object data);
    }
}
