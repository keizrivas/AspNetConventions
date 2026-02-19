using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses.Models;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Responses.Builders
{
    /// <summary>
    /// Provides a standard implementation of the IResponseBuilder interface for constructing responses with
    /// consistent formatting.
    /// </summary>
    /// <param name="options">The convention options for response formatting.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    internal sealed class DefaultApiResponseBuilder(AspNetConventionOptions options, ILogger logger) : ResponseAdapter(options, logger), IResponseBuilder
    {
        /// <summary>
        /// Determines if the specified value object is already a wrapped response of the expected type.
        /// </summary>
        /// <param name="value">The value object to check.</param>
        /// <returns>true if the value is already a DefaultApiResponse; otherwise, false.</returns>
        public override bool IsWrappedResponse(object? value)
        {
            return value is DefaultApiResponse;
        }

        /// <summary>
        /// Builds a standardized API response from the provided request result.
        /// </summary>
        /// <param name="apiResult">The request result containing response data and metadata.</param>
        /// <param name="requestDescriptor">The descriptor containing request context information.</param>
        /// <returns>A wrapped response object conforming to the standard API response format.</returns>
        public object BuildResponse(ApiResult apiResult, RequestDescriptor requestDescriptor)
        {
            var value = apiResult.GetValue();
            return new DefaultApiResponse(apiResult.StatusCode)
            {
                Data = value,
                Message = apiResult.Message,
                Metadata = apiResult.Metadata,
                Pagination = apiResult.Pagination,
            };
        }
    }
}
