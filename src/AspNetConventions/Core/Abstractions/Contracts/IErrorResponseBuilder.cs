using System;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for building error responses.
    /// </summary>
    public interface IErrorResponseBuilder : IResponseAdapter
    {
        /// <summary>
        /// Builds a response object from error mapping result.
        /// </summary>
        object BuildResponse(
            RequestResult result,
            Exception? exception,
            RequestDescriptor requestDescriptor);
    }
}
