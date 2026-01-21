using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AspNetConventions.Common.Hooks;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Builders;
using AspNetConventions.ExceptionHandling.Mappers;
using AspNetConventions.Http;

namespace AspNetConventions.Configuration
{
    /// <summary>
    /// Provides configuration options for exception handling.
    /// </summary>
    public sealed class ExceptionHandlingOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether exception handling is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include exception details in responses.
        /// When null, defaults to IsDevelopment environment check.
        /// </summary>
        public bool? IncludeExceptionDetails { get; set; }

        /// <summary>
        /// Gets or sets whether to include stack traces in responses.
        /// When null, defaults to IsDevelopment environment check.
        /// </summary>
        public bool? IncludeStackTrace { get; set; }

        /// <summary>
        /// Gets or sets the list of custom exception mappers.
        /// </summary>
        public IList<IExceptionMapper> Mappers { get; private set; } = [];

        /// <summary>
        /// Gets or sets the HTTP status codes that shouldn't be formatted.
        /// </summary>
        public HashSet<int> ExcludeStatusCodesToFormat { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Exception that shouldn't be formatted.
        /// </summary>
        public HashSet<Exception> ExcludeExceptionToFormat { get; private set; } = [];

        /// <summary>
        /// Gets or sets the default HTTP status code for unhandled exceptions.
        /// </summary>
        public HttpStatusCode DefaultStatusCode { get; set; } = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets or sets the default error code for unhandled exceptions.
        /// </summary>
        public string DefaultErrorCode { get; set; } = "INTERNAL_ERROR";

        /// <summary>
        /// Gets or sets the default error message for unhandled exceptions.
        /// </summary>
        public string DefaultErrorMessage { get; set; } = "An unexpected error occurred.";

        /// <summary>
        /// Gets or sets the default error message for validation exceptions.
        /// </summary>
        public string DefaultValidationMessage { get; set; } = "One or more validation errors occurred.";

        /// <summary>
        /// Gets or sets a custom exception response builder.
        /// </summary>
        public IExceptionResponseBuilder? CustomResponseBuilder { get; set; }

        /// <summary>
        /// Gets or sets the collection of hooks for customizing exception handling behavior.
        /// </summary>
        public ExceptionHandlingHooks Hooks { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="ResponseFormattingOptions"/> instance.
        /// </summary>
        internal IExceptionResponseBuilder GetResponseBuilder(AspNetConventionOptions options)
        {
            return CustomResponseBuilder
                ?? new StandardExceptionResponseBuilder(options);
        }

        /// <summary>
        /// Creates a deep clone of <see cref="ExceptionHandlingOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            return new ExceptionHandlingOptions
            {
                IsEnabled = IsEnabled,
                IncludeExceptionDetails = IncludeExceptionDetails,
                IncludeStackTrace = IncludeStackTrace,
                Mappers = Mappers,
                ExcludeStatusCodesToFormat = ExcludeStatusCodesToFormat,
                ExcludeExceptionToFormat = ExcludeExceptionToFormat,
                DefaultStatusCode = DefaultStatusCode,
                DefaultErrorCode = DefaultErrorCode,
                DefaultErrorMessage = DefaultErrorMessage,
                CustomResponseBuilder = CustomResponseBuilder,
                Hooks = Hooks,
            };
        }

        /// <summary>
        /// Gets the appropriate exception mapper for the given context.
        /// </summary>
        internal IExceptionMapper GetExceptionMapper(
            ExceptionDescriptor exceptionDescriptor,
            RequestDescriptor requestDescriptor)
        {
            // Try custom mappers first
            var customMapper = Mappers
                .FirstOrDefault(m => m.CanMapException(exceptionDescriptor, requestDescriptor));

            if (customMapper != null)
            {
                return customMapper;
            }

            // Fall back to standard mapper
            return new StandardExceptionMapper();
        }
    }
}
