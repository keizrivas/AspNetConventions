using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Contains metadata about the HTTP request and response context.
    /// </summary>
    /// <remarks>
    /// This metadata is included in API responses when <see cref="ResponseFormattingOptions.IncludeMetadata"/> is enabled.
    /// It provides diagnostic information about the request, including timing, trace information, and error details.
    /// </remarks>
    public sealed class Metadata
    {
        /// <summary>
        /// Gets or sets the HTTP method used for the request.
        /// </summary>
        /// <value>Examples include "GET", "POST", "PUT", "DELETE", etc.</value>
        public string? RequestType { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the response was generated.
        /// </summary>
        /// <value>Typically set to the UTC time when the response processing began.</value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the trace identifier for correlating requests across distributed systems.
        /// </summary>
        /// <value>The same trace ID as provided by ASP.NET Core's tracing infrastructure.</value>
        public string? TraceId { get; set; }

        /// <summary>
        /// Gets or sets the request path.
        /// </summary>
        /// <value>The relative path of the request, excluding the base URL.</value>
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets the exception type when an error occurred.
        /// </summary>
        /// <value>The name of the exception type when included in development or error responses.</value>
        /// <remarks>Only populated when error responses include exception type information.</remarks>
        public string? ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the stack trace information when an exception occurred.
        /// </summary>
        /// <value>A collection of stack frame details when stack traces are included in responses.</value>
        /// <remarks>Only populated in development environments when stack traces are enabled.</remarks>
        public HashSet<StackFrameInfo>? StackTrace { get; internal set; }

        /// <summary>
        /// Converts the specified <see cref="RequestDescriptor"/> to an <see cref="Metadata"/> instance.
        /// </summary>
        /// <param name="requestDescriptor">The HTTP request context to convert.</param>
        /// <returns>An <see cref="Metadata"/> representing the provided HTTP request context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestDescriptor"/> is null.</exception>
        /// <remarks>This explicit operator provides a convenient way to convert request descriptors to metadata objects.</remarks>
        public static explicit operator Metadata(RequestDescriptor requestDescriptor)
        {
            ArgumentNullException.ThrowIfNull(requestDescriptor);

            return new Metadata
            {
                Path = requestDescriptor.Path,
                Timestamp = requestDescriptor.Timestamp,
                TraceId = requestDescriptor.TraceId,
                RequestType = requestDescriptor.Method,
            };
        }

        /// <summary>
        /// Converts the specified <see cref="RequestDescriptor"/> to an <see cref="Metadata"/> instance.
        /// </summary>
        /// <param name="requestDescriptor">The HTTP request context to convert.</param>
        /// <returns>An <see cref="Metadata"/> representing the provided HTTP request context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestDescriptor"/> is null.</exception>
        /// <remarks>This method provides an alternative to the explicit operator for converting request descriptors to metadata.</remarks>
        public static Metadata ToMetadata(RequestDescriptor requestDescriptor)
        {
            return (Metadata)requestDescriptor;
        }
    }
}
