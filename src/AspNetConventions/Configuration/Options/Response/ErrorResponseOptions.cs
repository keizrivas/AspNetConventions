using System;
using System.Collections.Generic;
using System.Net;

namespace AspNetConventions.Configuration.Options.Response
{
    public sealed class ErrorResponseOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets the default HTTP status code for unhandled exceptions.
        /// </summary>
        public HttpStatusCode DefaultStatusCode { get; set; } = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets or sets the default error code for unhandled exceptions.
        /// </summary>
        public string DefaultErrorType { get; set; } = "UNEXPECTED_ERROR";

        /// <summary>
        /// Gets or sets the default error message for unhandled exceptions.
        /// </summary>
        public string DefaultErrorMessage { get; set; } = "An unexpected error occurred.";

        /// <summary>
        /// Gets or sets the default error message for validation exceptions.
        /// </summary>
        public string DefaultValidationMessage { get; set; } = "One or more validation errors occurred.";

        /// <summary>
        /// Gets the collection of allowed ProblemDetails extension keys that can be included in error responses.
        /// </summary>
        public HashSet<string> AllowedProblemDetailsExtensions { get; private set; } = new(StringComparer.Ordinal)
        {
            "code",
            "details"
        };

        /// <summary>
        /// Gets or sets a value indicating whether the exception type should be included in the output.
        /// </summary>
        public bool? IncludeExceptionType { get; set; }

        /// <summary>
        /// Include stack trace in error responses (typically only in Development)
        /// </summary>
        public bool? IncludeStackTrace { get; set; }

        /// <summary>
        /// Maximum depth for nested stack trace
        /// </summary>
        public int MaxStackTraceDepth { get; set; } = 50;

        /// <summary>
        /// Creates a deep clone of <see cref="ErrorResponseOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ErrorResponseOptions"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            var cloned = (ErrorResponseOptions)MemberwiseClone();
            cloned.AllowedProblemDetailsExtensions = new HashSet<string>(
                AllowedProblemDetailsExtensions, 
                StringComparer.Ordinal
            );

            return cloned;
        }
    }
}
