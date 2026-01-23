using System;
using System.Collections;
using System.Collections.Generic;
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
                Type = result.Type ?? Options.ExceptionHandling.DefaultErrorCode,
                Message = result.Message,
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
