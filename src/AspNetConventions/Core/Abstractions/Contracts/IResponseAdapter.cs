using AspNetConventions.Responses.Models;

namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for adapting and detecting wrapped response objects.
    /// </summary>
    /// <remarks>
    /// This interface provides the ability to identify whether a data object has already been wrapped
    /// by AspNetConventions response formatting, preventing double-wrapping and maintaining response integrity.
    /// </remarks>
    public interface IResponseAdapter
    {
        /// <summary>
        /// Determines whether the specified data object represents a wrapped response.
        /// </summary>
        /// <param name="data">The data object to evaluate.</param>
        /// <returns>true if the data object is recognized as a wrapped response; otherwise, false.</returns>
        /// <remarks>
        /// This method checks if the data object matches the structure of AspNetConventions wrapped responses,
        /// such as <see cref="DefaultApiResponse{TData}"/>, <see cref="DefaultApiErrorResponse{TError}"/> or custom wrapped response types.
        /// </remarks>
        bool IsWrappedResponse(object? data);
    }
}
