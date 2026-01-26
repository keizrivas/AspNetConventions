using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Encapsulates a standard exception structure for responses.
    /// </summary>
    /// <remarks>Use this class to provide a consistent envelope for exception.</remarks>
    public sealed class ExceptionDescriptor
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
        /// Gets or sets the response data.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets whether this exception should be logged.
        /// </summary>
        public bool ShouldLog { get; set; } = true;

        public LogLevel LogLevel { get; set; } = LogLevel.Error;

        internal Exception? Exception { get; set; }
    }
}
