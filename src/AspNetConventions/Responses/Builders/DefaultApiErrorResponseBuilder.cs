using System;
using System.Collections.Concurrent;
using System.Net;
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
    internal sealed class DefaultApiErrorResponseBuilder(AspNetConventionOptions options, ILogger logger) : ResponseAdapter(options, logger), IErrorResponseBuilder
    {
        private static int _lastLoggedCount;
        private static readonly ConcurrentDictionary<Type, Func<RequestResult, ApiResponse>> _factoryCache
            = new(concurrencyLevel: Environment.ProcessorCount, capacity: 50);

        public override bool IsWrappedResponse(object? data)
        {
            if (data is null)
            {
                return false;
            }

            var type = data.GetType();
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(DefaultApiErrorResponse<>);
        }

        public object BuildResponse(RequestResult requestResult, Exception? exception, RequestDescriptor requestDescriptor)
        {
            // Get the data type from the request result
            var dataType = requestResult.Data?.GetType() ?? typeof(object);

            // Get or create the factory function for the specific data type
            var factory = _factoryCache.GetOrAdd(dataType, type =>
            {
                var responseType = typeof(DefaultApiErrorResponse<>).MakeGenericType(type);

                var ctor = responseType.GetConstructor([typeof(HttpStatusCode), typeof(object)])
                    ?? throw new InvalidOperationException("Required constructor not found.");

                var typeProp = responseType.GetProperty(nameof(DefaultApiErrorResponse<object>.Type))!;
                var messageProp = responseType.GetProperty(nameof(DefaultApiErrorResponse<object>.Message))!;
                var metadataProp = responseType.GetProperty(nameof(DefaultApiErrorResponse<object>.Metadata))!;

                var result = MonitorCacheSize(_factoryCache.Count, _lastLoggedCount);
                if (result.HasValue)
                {
                    _lastLoggedCount = result.Value;
                }

                return requestResult =>
                {
                    // Create instance using required constructor
                    var instance = (ApiResponse)ctor.Invoke([requestResult.StatusCode, requestResult.Data]);

                    // Set properties
                    typeProp.SetValue(instance, requestResult.Type);
                    messageProp.SetValue(instance, requestResult.Message);
                    metadataProp.SetValue(instance, requestResult.Metadata);

                    return instance;
                };
            });

            return factory(requestResult);
        }
    }
}
