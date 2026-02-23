using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Encapsulates a standard exception structure for responses.
    /// </summary>
    public class ExceptionDescriptor
    {
        /// <summary>
        /// Gets or sets the default HTTP status code.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the exception type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the response value.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets whether this exception should be logged.
        /// </summary>
        public bool ShouldLog { get; set; } = true;

        /// <summary>
        /// Gets or sets the log level for this exception.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Error;

        /// <summary>
        /// Gets or sets the original exception.
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Encapsulates a standard exception structure for responses.
    /// </summary>
    /// <typeparam name="TValue">The type of value included in the exception descriptor.</typeparam>
    public class ExceptionDescriptor<TValue> : ExceptionDescriptor
    {
        /// <summary>
        /// Gets or sets the response value.
        /// </summary>
        public new TValue? Value { get; set; }
    }
}
