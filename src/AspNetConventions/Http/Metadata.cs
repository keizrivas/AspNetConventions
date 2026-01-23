using System;
using System.Collections.Generic;
using AspNetConventions.ExceptionHandling.Models;

namespace AspNetConventions.Http
{
    /// <summary>
    /// Metadata about the HTTP response.
    /// </summary>
    public sealed class Metadata
    {
        /// <summary>
        /// Gets or sets the timestamp when the response was generated.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the trace identifier.
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method used.
        /// </summary>
        public string? RequestType { get; set; }

        /// <summary>
        /// Gets or sets the request path.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets the exception type.
        /// </summary>
        public string? ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        public HashSet<StackFrameInfo>? StackTrace { get; internal set; }

        /// <summary>
        /// Converts the specified <see cref="RequestDescriptor"/> to an <see cref="Metadata"/> instance.
        /// </summary>
        /// <param name="requestDescriptor">The HTTP request context to convert.</param>
        /// <returns>An <see cref="Metadata"/> representing the provided HTTP request context.</returns>
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
        public static Metadata ToMetadata(RequestDescriptor requestDescriptor)
        {
            return (Metadata)requestDescriptor;
        }
    }
}
