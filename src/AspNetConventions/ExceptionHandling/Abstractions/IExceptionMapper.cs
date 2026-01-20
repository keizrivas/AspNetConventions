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
        bool CanMapException(ExceptionDescriptor exceptionContext, RequestDescriptor httpContext);

        /// <summary>
        /// Maps an exception to a standardized error response.
        /// </summary>
        ExceptionEnvelope MapException(ExceptionDescriptor exceptionContext, RequestDescriptor httpContext);
    }
}
