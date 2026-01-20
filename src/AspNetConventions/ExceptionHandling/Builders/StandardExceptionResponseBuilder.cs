using AspNetConventions.Common.Abstractions;
using AspNetConventions.Configuration;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ExceptionHandling.Builders
{
    /// <summary>
    /// Standard builder for exception responses.
    /// </summary>
    internal sealed class StandardExceptionResponseBuilder(AspNetConventionOptions options) : ResponseAdapter(options), IExceptionResponseBuilder
    {
        public override bool IsWrappedResponse(object? data)
        {
            return data is StandardExceptionResponse;
        }

        public object BuildResponse(ExceptionEnvelope exceptionEnvelope, ExceptionDescriptor exceptionDescriptor, RequestDescriptor requestDescriptor)
        {
            // Create standard response
            var response = new StandardExceptionResponse(exceptionEnvelope.StatusCode)
            {
                Type = exceptionEnvelope.ErrorCode,
                Message = exceptionEnvelope.Message,
                Data = exceptionEnvelope.Data,
                Metadata = exceptionEnvelope.Metadata,
            };

            return response;
        }
    }
}
