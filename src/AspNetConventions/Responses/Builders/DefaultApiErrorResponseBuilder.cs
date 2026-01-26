using System;
using System.Collections;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses.Models;

namespace AspNetConventions.Responses.Builders
{
    /// <summary>
    /// Standard builder for exception responses.
    /// </summary>
    internal sealed class DefaultApiErrorResponseBuilder(AspNetConventionOptions options) : ResponseAdapter(options), IErrorResponseBuilder
    {
        public override bool IsWrappedResponse(object? data)
        {
            return data is DefaultApiErrorResponse;
        }

        public object BuildResponse(RequestResult result, Exception? exception, RequestDescriptor requestDescriptor)
        {
            // Create standard response
            var response = new DefaultApiErrorResponse(result.StatusCode)
            {
                Type = result.Type ?? Options.Response.ErrorResponse.DefaultErrorType,
                Message = result.Message ?? Options.Response.ErrorResponse.DefaultErrorMessage,
                Metadata = result.Metadata,
            };

            if (result.Data is IEnumerable enumerable && result.Data is not string)
            {
                foreach (var item in enumerable)
                {
                    response.Errors.Add(item);
                }
            }
            else if (result.Data is not null)
            {
                response.Errors.Add(result.Data);
            }

            return response;
        }
    }
}
