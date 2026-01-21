using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AspNetConventions.ExceptionHandling.Models;

namespace AspNetConventions.Http
{
    /// <summary>
    /// Describes an request exception and its associated stack trace within the current application context.
    /// </summary>
    /// <remarks>The <see cref="ExceptionDescriptor"/> class provides information about an exception that occurred.
    /// All properties are initialized from the provided <see cref="Exception"/> at the time of construction and represent
    /// the data of the current exception.</remarks>
    public sealed class ExceptionDescriptor(Exception exception)
    {
        public ExceptionDescriptor(Exception exception, HttpStatusCode statusCode)
            : this(exception)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the exception that occurred.
        /// </summary>
        public Exception Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));

        /// <summary>
        /// Gets the suggested HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        public string Message { get; init; } = exception.Message;

        /// <summary>
        /// Gets the inner exception message, if any.
        /// </summary>
        public string? InnerMessage { get; init; } = exception.InnerException?.Message;

        /// <summary>
        /// Gets or sets the exception type.
        /// </summary>
        public string? ExceptionType { get; set; } = exception.GetType().Name;

        /// <summary>
        /// Gets the stack trace information.
        /// </summary>
        public HashSet<StackTraceInfo>? StackTrace => GetStackTrace(Exception);

        /// <summary>
        /// Gets the raw stack trace information.
        /// </summary>
        public string? RawStackTrace => exception.StackTrace;

        private static HashSet<StackTraceInfo> GetStackTrace(Exception ex)
        {
            var trace = new System.Diagnostics.StackTrace(ex, true);

            return trace.GetFrames()?
                .Select(f => new StackTraceInfo(
                    Method: f.GetMethod()?.ToString() ?? "Unknow",
                    File: f.GetFileName(),
                    Line: f.GetFileLineNumber() == 0 ? null : f.GetFileLineNumber()
                )).ToHashSet()
                ?? [];
        }

        /// <summary>
        /// Converts the specified <see cref="ExceptionDescriptor"/> to an <see cref="ExceptionEnvelope"/> instance.
        /// </summary>
        /// <param name="context">The HTTP exception context to convert.</param>
        /// <returns>An <see cref="ExceptionEnvelope"/> representing the provided HTTP exception context.</returns>
        public static explicit operator ExceptionEnvelope(ExceptionDescriptor context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return new ExceptionEnvelope
            {
                Message = context.Message,
                StatusCode = context.StatusCode,
                ErrorCode = null,
            };
        }

        /// <summary>
        /// Converts the specified <see cref="ExceptionDescriptor"/> to an <see cref="ExceptionEnvelope"/> instance.
        /// </summary>
        /// <param name="context">The HTTP exception context to convert.</param>
        /// <returns>An <see cref="ExceptionEnvelope"/> representing the provided HTTP exception context.</returns>
        public static ExceptionEnvelope ToExceptionEnvelope(ExceptionDescriptor context)
        {
            return (ExceptionEnvelope)context;
        }
    }
}
