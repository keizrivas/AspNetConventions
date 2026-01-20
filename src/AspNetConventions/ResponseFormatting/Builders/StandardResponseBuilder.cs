using AspNetConventions.Common.Abstractions;
using AspNetConventions.Configuration;
using AspNetConventions.Http;
using AspNetConventions.ResponseFormatting.Abstractions;
using AspNetConventions.ResponseFormatting.Models;

namespace AspNetConventions.ResponseFormatting.Builders
{
    /// <summary>
    /// Provides a standard implementation of the IResponseBuilder interface for constructing responses with
    /// consistent formatting.
    /// </summary>
    internal sealed class StandardResponseBuilder(AspNetConventionOptions options) : ResponseAdapter(options), IResponseBuilder
    {
        public override bool IsWrappedResponse(object? data)
        {
            return data is StandardResponse;
        }

        public object BuildResponse(ResponseEnvelope responseEnvelope, RequestDescriptor requestDescriptor)
        {
            // Create standard response
            var response = new StandardResponse(responseEnvelope.StatusCode)
            {
                Data = responseEnvelope.Data,
                Message = responseEnvelope.Message,
                Metadata = responseEnvelope.Metadata,
                Pagination = responseEnvelope.Pagination,
            };

            return response;
        }
    }
}
