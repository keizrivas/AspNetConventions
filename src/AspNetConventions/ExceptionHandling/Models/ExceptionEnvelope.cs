using System.Net;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Http;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Encapsulates a standard exception structure for responses.
    /// </summary>
    /// <remarks>Use this class to provide a consistent envelope for exception.</remarks>
    public sealed class ExceptionEnvelope : IResponseEnvelope
    {
        public ExceptionEnvelope() { }

        public ExceptionEnvelope(object? data)
        {
            Data = data;
        }

        public ExceptionEnvelope(object? data, string? message)
            : this(data)
        {
            Message = message;
        }

        public ExceptionEnvelope(object? data, string? message, HttpStatusCode statusCode)
            : this(data, message)
        {
            StatusCode = statusCode;
        }

        public ExceptionEnvelope(object? data, HttpStatusCode statusCode)
            : this(data, null, statusCode)
        {
        }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public object? Data { get; private set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        public Metadata? Metadata { get; private set; }

        /// <summary>
        /// Gets or sets the application-specific error code.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets whether this exception should be logged.
        /// </summary>
        public bool ShouldLog { get; set; } = true;

        /// <summary>
        /// Sets the data property.
        /// <param name="data">The data object to associate with this instance.</param>
        /// </summary>
        public void SetData(object? data) => Data = data;

        /// <summary>
        /// Sets the metadata associated with this instance.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        public void SetMetadata(Metadata? metadata) => Metadata = metadata;

        /// <summary>
        /// Attempts to log an exception and related error details using the specified logger.
        /// </summary>
        /// <param name="logger">The logger instance used to record the error information.</param>
        /// <param name="exception">The exception to be logged.</param>
        /// <returns>true if the error information was successfully logged; otherwise, false.</returns>
        internal bool LogException(ILogger? logger, System.Exception? exception = null)
        {
            if (!ShouldLog || logger == null)
            {
                return false;
            }

            var logMessage = $"Exception occurred: StatusCode={StatusCode}, ErrorCode={ErrorCode}, Message={Message}";

            if (exception != null)
            {
                logger.LogError(exception, logMessage);
            }
            else
            {
                logger.LogError(logMessage);
            }

            return true;
        }
    }
}
