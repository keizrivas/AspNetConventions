using AspNetConventions.Common.Abstractions;
using AspNetConventions.Http;
using AspNetConventions.ResponseFormatting.Models;

namespace AspNetConventions.ResponseFormatting.Abstractions
{
    /// <summary>
    /// Defines a contract for building standardized responses.
    /// </summary>
    public interface IResponseBuilder: IResponseAdapter
    {
        /// <summary>
        /// Builds a response object from data and HTTP context.
        /// </summary>
        object BuildResponse(ResponseEnvelope responseEnvelope, RequestDescriptor requestDescriptor);
    }
}
