using System;
using System.Collections.Concurrent;
using System.Net;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Extensions;
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
        /// Tracks the last logged cache size count for monitoring purposes.
        /// </summary>
        private static int _lastLoggedCount;

        /// <summary>
        /// A thread-safe cache of factory functions for creating typed response objects.
        /// </summary>
        /// <remarks>
        /// This cache improves performance by storing compiled factory functions for each data type
        /// encountered, avoiding the overhead of reflection and expression compilation on subsequent requests.
        /// </remarks>
        private static readonly ConcurrentDictionary<Type, Func<RequestResult, ApiResponse>> _factoryCache
            = new(concurrencyLevel: Environment.ProcessorCount, capacity: 50);

        /// <summary>
        /// Determines if the specified data object is already a wrapped response of the expected type.
        /// </summary>
        /// <param name="data">The data object to check.</param>
        /// <returns>true if the data is already a DefaultApiResponse; otherwise, false.</returns>
        public override bool IsWrappedResponse(object? data)
        {
            if (data is null)
            {
                return false;
            }

            var type = data.GetType();
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(DefaultApiResponse<>);
        }

        /// <summary>
        /// Builds a standardized API response from the provided request result.
        /// </summary>
        /// <param name="requestResult">The request result containing response data and metadata.</param>
        /// <param name="requestDescriptor">The descriptor containing request context information.</param>
        /// <returns>A wrapped response object conforming to the standard API response format.</returns>
        public object BuildResponse(RequestResult requestResult, RequestDescriptor requestDescriptor)
        {
            // Get the data type from the request result
            var dataType = requestResult.Data?.GetType() ?? typeof(object);

            // Get or create the factory function for the specific data type
            var factory = _factoryCache.GetOrAdd(dataType, type =>
            {
                var responseType = typeof(DefaultApiResponse<>).MakeGenericType(type);

                var ctor = responseType.GetConstructor([typeof(HttpStatusCode)])
                    ?? throw new InvalidOperationException("Required constructor not found.");

                var dataProp = responseType.GetProperty(nameof(DefaultApiResponse<object>.Data))!;
                var messageProp = responseType.GetProperty(nameof(DefaultApiResponse<object>.Message))!;
                var metadataProp = responseType.GetProperty(nameof(DefaultApiResponse<object>.Metadata))!;
                var paginationProp = responseType.GetProperty(nameof(DefaultApiResponse<object>.Pagination))!;

                var result = Logger.LogCacheSize("_factoryCache", _factoryCache.Count, _lastLoggedCount);
                if (result.HasValue)
                {
                    _lastLoggedCount = result.Value;
                }

                return requestResult =>
                {
                    // Create instance using required constructor
                    var instance = (ApiResponse)ctor.Invoke([requestResult.StatusCode]);

                    // Set properties
                    dataProp.SetValue(instance, requestResult.Data);
                    messageProp.SetValue(instance, requestResult.Message);
                    metadataProp.SetValue(instance, requestResult.Metadata);
                    paginationProp.SetValue(instance, requestResult.Pagination);

                    return instance;
                };
            });

            return factory(requestResult);
        }
    }
}
