using AspNetConventions.Common.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ExceptionHandling.Abstractions
{
    /// <summary>
    /// Defines a contract for building exception responses.
    /// </summary>
    public interface IExceptionResponseBuilder : IResponseAdapter
    {
        /// <summary>
        /// Builds a response object from exception mapping result.
        /// </summary>
        object BuildResponse(
            ExceptionEnvelope result,
            ExceptionDescriptor? exceptionDescriptor,
            RequestDescriptor requestDescriptor);
    }
}
