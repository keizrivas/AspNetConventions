using System;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ExceptionHandling.Abstractions
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
