using System;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ExceptionHandling.Abstractions
{
    /// <summary>
    /// Defines a contract for mapping exceptions to HTTP responses.
    /// </summary>
    public interface IExceptionMapper
    {
        /// <summary>
        /// Determines whether this mapper can handle the specified exception.
        /// </summary>
        bool CanMapException(Exception exception, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Maps an exception to a standardized error response.
        /// </summary>
        ExceptionDescriptor2 MapException(Exception exception, RequestDescriptor requestDescriptor);
    }
}
