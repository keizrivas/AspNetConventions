using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for building standardized responses.
    /// </summary>
    public interface IResponseBuilder : IResponseAdapter
    {
        /// <summary>
        /// Builds a response object from data and HTTP context.
        /// </summary>
        object BuildResponse(RequestResult requestResult, RequestDescriptor requestDescriptor);
    }
}
