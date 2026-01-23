using AspNetConventions.Common.Abstractions;
using AspNetConventions.Configuration;
using AspNetConventions.Http;
using AspNetConventions.ResponseFormatting.Abstractions;
using AspNetConventions.ResponseFormatting.Models;
using Microsoft.Extensions.Options;

namespace AspNetConventions.ResponseFormatting.Builders
{
    /// <summary>
    /// Provides a standard implementation of the IResponseBuilder interface for constructing responses with
    /// consistent formatting.
    /// </summary>
    internal sealed class DefaultApiResponseBuilder(AspNetConventionOptions options) : ResponseAdapter(options), IResponseBuilder
    {
        public override bool IsWrappedResponse(object? data)
        {
            return data is DefaultApiResponse;
        }

        public object BuildResponse(RequestResult requestResult, RequestDescriptor requestDescriptor)
        {
            // Create standard response
            var response = new DefaultApiResponse(requestResult.StatusCode)
            {
                Data = requestResult.Data,
                Message = requestResult.Message,
                Metadata = requestResult.Metadata,
                Pagination = requestResult.Pagination,
            };

            return response;
        }
    }
}
