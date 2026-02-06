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
    /// <param name="options">The convention options for response formatting.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    internal sealed class DefaultApiErrorResponseBuilder(AspNetConventionOptions options, ILogger logger) : ResponseAdapter(options, logger), IErrorResponseBuilder
    {
        /// <summary>
        /// Tracks the last logged cache size count for monitoring purposes.
        /// </summary>
        private static int _lastLoggedCount;

        /// <summary>
        /// A thread-safe cache of factory functions for creating typed error response objects.
        /// </summary>
        /// <remarks>
        /// This cache improves performance by storing compiled factory functions for each data type
        /// encountered in error responses, avoiding the overhead of reflection and expression compilation
        /// on subsequent error handling requests.
        /// </remarks>
        private static readonly ConcurrentDictionary<Type, Func<RequestResult, ApiResponse>> _factoryCache
            = new(concurrencyLevel: Environment.ProcessorCount, capacity: 50);

        /// <summary>
        /// Determines if the specified data object is already a wrapped error response of the expected type.
        /// </summary>
        /// <param name="data">The data object to check.</param>
        /// <returns>true if the data is already a DefaultApiErrorResponse; otherwise, false.</returns>
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

        /// <summary>
        /// Builds a standardized API error response from the provided request result and exception.
        /// </summary>
        /// <param name="requestResult">The request result containing error data and metadata.</param>
        /// <param name="exception">The exception that caused the error response (may be null).</param>
        /// <param name="requestDescriptor">The descriptor containing request context information.</param>
        /// <returns>A wrapped error response object conforming to the standard API error response format.</returns>
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
