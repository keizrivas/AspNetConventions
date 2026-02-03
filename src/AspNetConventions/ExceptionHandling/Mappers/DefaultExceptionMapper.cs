using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.ExceptionHandling.Mappers
{
    /// <summary>
    /// Standard exception mapper for common .NET exceptions.
    /// </summary>
    internal sealed class DefaultExceptionMapper : IExceptionMapper
    {
        private static readonly Dictionary<Type, (HttpStatusCode StatusCode, string ErrorType)> _exceptionMappings = new()
        {
            // Argument & validation
            [typeof(ArgumentNullException)] = (HttpStatusCode.BadRequest, "ARGUMENT_NULL"),
            [typeof(ArgumentOutOfRangeException)] = (HttpStatusCode.BadRequest, "ARGUMENT_OUT_OF_RANGE"),
            [typeof(ArgumentException)] = (HttpStatusCode.BadRequest, "INVALID_ARGUMENT"),
            [typeof(ValidationException)] = (HttpStatusCode.BadRequest, "VALIDATION_ERROR"),

            // Authorization & authentication
            [typeof(UnauthorizedAccessException)] = (HttpStatusCode.Unauthorized, "UNAUTHORIZED"),
            [typeof(SecurityException)] = (HttpStatusCode.Forbidden, "FORBIDDEN"),

            // Resource lookup
            [typeof(KeyNotFoundException)] = (HttpStatusCode.NotFound, "NOT_FOUND"),
            [typeof(FileNotFoundException)] = (HttpStatusCode.NotFound, "FILE_NOT_FOUND"),
            [typeof(DirectoryNotFoundException)] = (HttpStatusCode.NotFound, "DIRECTORY_NOT_FOUND"),

            // State & lifecycle
            [typeof(InvalidOperationException)] = (HttpStatusCode.Conflict, "INVALID_OPERATION"),
            [typeof(ObjectDisposedException)] = (HttpStatusCode.Gone, "OBJECT_DISPOSED"),

            // Infrastructure & runtime
            [typeof(NotImplementedException)] = (HttpStatusCode.NotImplemented, "NOT_IMPLEMENTED"),
            [typeof(TimeoutException)] = (HttpStatusCode.RequestTimeout, "TIMEOUT"),
            [typeof(TaskCanceledException)] = (HttpStatusCode.RequestTimeout, "REQUEST_CANCELLED"),
            [typeof(OperationCanceledException)] = (HttpStatusCode.RequestTimeout, "OPERATION_CANCELLED"),

            // Fallback
            [typeof(Exception)] = (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR")
        };

        public bool CanMapException(Exception exception, RequestDescriptor requestDescriptor)
        {
            return _exceptionMappings.ContainsKey(exception.GetType());
        }

        public ExceptionDescriptor MapException(Exception exception, RequestDescriptor requestDescriptor)
        {
            var (statusCode, errorCode) = GetMapping(exception);
 
            var result = new ExceptionDescriptor
            {
                StatusCode = statusCode,
                Type = errorCode,
            };

            // Handle validation exceptions specially
            if (TryGetValidationErrors(exception, out var validationErrors))
            {
                result.Data = validationErrors;
            }

            return result;
        }

        /// <summary>
        /// Retrieves the HTTP status code and error code mapping for the specified exception type
        /// </summary>
        /// <returns>A tuple containing the mapped HTTP status code and error code.</returns>
        private static (HttpStatusCode? StatusCode, string? ErrorCode) GetMapping(Exception exception)
        {
            return _exceptionMappings.TryGetValue(exception.GetType(), out var mapping)
                ? mapping
                : (null, null);
        }

        /// <summary>
        /// Attempts to extract validation errors from the specified exception if it represents a validation failure.
        /// </summary>
        /// <param name="exception">The exception to inspect for validation errors.</param>
        /// <param name="errors">The lists of validation error messages.</param>
        /// <returns>true if validation errors were successfully extracted from the exception; otherwise, false.</returns>
        private static bool TryGetValidationErrors(
            Exception exception,
            out Dictionary<string, HashSet<string>>? errors)
        {
            errors = null;

            // Support built-in ValidationException
            if (exception is ValidationException validationEx &&
                validationEx.ValidationResult is { ErrorMessage: not null })
            {
                errors = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
                var message = validationEx.ValidationResult.ErrorMessage;

                foreach (var memberName in validationEx.ValidationResult.MemberNames)
                {
                    if (!errors.TryGetValue(memberName, out var messages))
                    {
                        messages = new HashSet<string>(StringComparer.Ordinal);
                        errors.Add(memberName, messages);
                    }

                    messages.Add(message);
                }

                return errors.Count > 0;
            }

            return false;
        }
    }
}
