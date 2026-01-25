using System;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Http.Services;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Responses
{
    internal class ResponseManager(
        AspNetConventionOptions options,
        RequestDescriptor requestDescriptor,
        ILogger logger) : ResponseAdapter(options)
    {

        private readonly IResponseBuilder _responseBuilder = options.Response.GetResponseBuilder(options);
        private readonly IErrorResponseBuilder _errorResponseBuilder = options.Response.GetErrorResponseBuilder(options);

        public override bool IsWrappedResponse(object? data)
        {
            throw new NotImplementedException();
        }
    }
}
