using System;
using System.Collections.Generic;
using AspNetConventions.Extensions;

namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Represents metadata information about an exception, including its type, message, and stack trace.
    /// </summary>
    /// <param name="exception">The exception instance to extract metadata from.</param>
    /// <param name="maxStackTraceDepth">The maximum number of stack trace frames to include in the metadata. Defaults to 25.</param>
    public class ExceptionMetadata(Exception exception, int maxStackTraceDepth = 25)
    {
        /// <summary>
        /// Gets the full name of the exception type.
        /// </summary>
        /// <value>A string representing the full name of the exception type, or "UnknownException" if the type information is unavailable.</value>
        public string Type { get; } = exception.GetType().FullName ?? "UnknownException";

        /// <summary>
        /// Gets the message associated with the exception.
        /// </summary>
        /// <value>A string containing the exception message, which provides details about the error that occurred.</value>
        public string Message { get; } = exception.Message;

        /// <summary>
        /// Gets a collection of stack trace frames from the exception, limited to a specified maximum depth.
        /// </summary>
        /// <value>A read-only list of strings, each representing a frame in the exception's stack trace, up to the specified maximum depth.</value>
        public IReadOnlyList<string> StackTrace { get; } = exception.GetStackTrace(maxStackTraceDepth);
    }
}
