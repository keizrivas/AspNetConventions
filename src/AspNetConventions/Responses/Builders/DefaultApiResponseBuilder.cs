using AspNetConventions.Configuration;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses.Models;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Responses.Builders
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
