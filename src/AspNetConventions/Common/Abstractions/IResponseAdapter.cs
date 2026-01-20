namespace AspNetConventions.Common.Abstractions
{
    public interface IResponseAdapter
    {
        /// <summary>
        /// Determines whether the specified data object represents a wrapped response.
        /// </summary>
        /// <param name="data">The data object to evaluate.</param>
        /// <returns>true if the data object is recognized as a wrapped response; otherwise, false.</returns>
        bool IsWrappedResponse(object? data);
    }
}
