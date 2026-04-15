using System;
using System.Collections.Generic;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Contains metadata about the HTTP request and response context.
    /// </summary>
    /// <remarks>
    /// This metadata is included in API responses when <c>ResponseFormattingOptions.IncludeMetadata</c> is enabled.
    /// It provides diagnostic information about the request, including timing, trace information, and error details.
    /// </remarks>
    public sealed class Metadata : Dictionary<string, object?>
    {
        /// <summary>Key used for the HTTP method.</summary>
        public const string RequestTypeKey = "RequestType";

        /// <summary>Key used for the UTC timestamp of the response.</summary>
        public const string TimestampKey = "Timestamp";

        /// <summary>Key used for the distributed trace identifier.</summary>
        public const string TraceIdKey = "TraceId";

        /// <summary>Key used for the request path.</summary>
        public const string PathKey = "Path";

        /// <summary>Key used for exception metadata when an error occurred.</summary>
        public const string ExceptionKey = "Exception";

        /// <summary>
        /// Converts the specified <see cref="RequestDescriptor"/> to a <see cref="Metadata"/> instance,
        /// pre-populated with the standard request fields.
        /// </summary>
        /// <param name="requestDescriptor">The HTTP request context to convert.</param>
        /// <returns>A <see cref="Metadata"/> representing the provided HTTP request context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestDescriptor"/> is null.</exception>
        public static explicit operator Metadata(RequestDescriptor requestDescriptor)
        {
            ArgumentNullException.ThrowIfNull(requestDescriptor);

            return new Metadata
            {
                [PathKey] = requestDescriptor.Path,
                [TimestampKey] = requestDescriptor.Timestamp,
                [TraceIdKey] = requestDescriptor.TraceId,
                [RequestTypeKey] = requestDescriptor.Method,
            };
        }

        /// <summary>
        /// Converts the specified <see cref="RequestDescriptor"/> to a <see cref="Metadata"/> instance.
        /// </summary>
        /// <param name="requestDescriptor">The HTTP request context to convert.</param>
        /// <returns>A <see cref="Metadata"/> representing the provided HTTP request context.</returns>
        public static Metadata ToMetadata(RequestDescriptor requestDescriptor) =>
            (Metadata)requestDescriptor;
    }
}
