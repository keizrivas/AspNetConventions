namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for adapting data objects into an IResponseCollection.
    /// </summary>
    /// <remarks>Implementations of this interface enable flexible conversion of various data formats or types
    /// into a standardized IResponseCollection.</remarks>
    public interface IResponseCollectionAdapter
    {
        /// <summary>
        /// Returns true if this adapter can convert the given data object into an IResponseCollection.
        /// </summary>
        bool CanHandle(object data);

        /// <summary>
        /// Converts the given data object into an IResponseCollection.
        /// </summary>
        IResponseCollection Convert(object data);
    }
}
