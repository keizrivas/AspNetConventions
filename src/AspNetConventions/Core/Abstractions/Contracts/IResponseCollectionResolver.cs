namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a mechanism for resolving an object into an <see cref="IResponseCollection"/> using registered adapters.
    /// </summary>
    public interface IResponseCollectionResolver
    {
        /// <summary>
        /// Tries to resolve the given data into an IResponseCollection using registered adapters.
        /// Returns null if no adapter can handle it.
        /// </summary>
        IResponseCollection? TryResolve(object data);
    }
}
