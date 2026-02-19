using System;
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
    /// Standard builder for exception responses.
    /// </summary>
    /// <param name="options">The convention options for response formatting.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    internal sealed class DefaultApiErrorResponseBuilder(AspNetConventionOptions options, ILogger logger) : ResponseAdapter(options, logger), IErrorResponseBuilder
    {
        /// <summary>
        /// Determines if the specified value object is already a wrapped error response of the expected type.
        /// </summary>
        /// <param name="value">The value object to check.</param>
        /// <returns>true if the value is already a DefaultApiErrorResponse; otherwise, false.</returns>
        public override bool IsWrappedResponse(object? value)
        {
            return value is DefaultApiErrorResponse;
        }

        /// <summary>
        /// Builds a standardized API error response from the provided request result and exception.
        /// </summary>
        /// <param name="apiResult">The request result containing error data and metadata.</param>
        /// <param name="exception">The exception that caused the error response (may be null).</param>
        /// <param name="requestDescriptor">The descriptor containing request context information.</param>
        /// <returns>A wrapped error response object conforming to the standard API error response format.</returns>
        public object BuildResponse(ApiResult apiResult, Exception? exception, RequestDescriptor requestDescriptor)
        {
            var value = apiResult.GetValue();
            return new DefaultApiErrorResponse(apiResult.StatusCode, value)
            {
                Type = apiResult.Type,
                Message = apiResult.Message,
                Metadata = apiResult.Metadata,
            };
        }
    }
}
