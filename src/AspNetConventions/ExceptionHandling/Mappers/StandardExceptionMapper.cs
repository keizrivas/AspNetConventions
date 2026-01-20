using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ExceptionHandling.Mappers
{
    /// <summary>
    /// Standard exception mapper for common .NET exceptions.
    /// </summary>
    internal sealed class StandardExceptionMapper : IExceptionMapper
    {
        private static readonly Dictionary<Type, (HttpStatusCode StatusCode, string ErrorCode)> _mappings = new()
        {
            [typeof(ArgumentNullException)] = (HttpStatusCode.BadRequest, "ARGUMENT_NULL"),
            [typeof(ArgumentException)] = (HttpStatusCode.BadRequest, "INVALID_ARGUMENT"),
            [typeof(InvalidOperationException)] = (HttpStatusCode.BadRequest, "INVALID_OPERATION"),
            [typeof(UnauthorizedAccessException)] = (HttpStatusCode.Unauthorized, "UNAUTHORIZED"),
            [typeof(KeyNotFoundException)] = (HttpStatusCode.NotFound, "NOT_FOUND"),
            [typeof(NotImplementedException)] = (HttpStatusCode.NotImplemented, "NOT_IMPLEMENTED"),
            [typeof(TimeoutException)] = (HttpStatusCode.RequestTimeout, "TIMEOUT"),
            [typeof(ValidationException)] = (HttpStatusCode.BadRequest, "VALIDATION_ERROR"),
        };

        public bool CanMapException(
            ExceptionDescriptor exceptionContext,
            RequestDescriptor httpContext)
        {
            return _mappings.ContainsKey(exceptionContext.Exception.GetType());
        }

        public ExceptionEnvelope MapException(
            ExceptionDescriptor exceptionContext,
            RequestDescriptor httpContext)
        {
            var result = (ExceptionEnvelope)exceptionContext;
            var (statusCode, errorCode) = GetMapping(exceptionContext.Exception);

            result.ErrorCode ??= errorCode;
            if (statusCode.HasValue)
            {
                result.StatusCode = statusCode.Value;
            }

            // Handle validation exceptions specially
            if (TryGetValidationErrors(exceptionContext.Exception, out var validationErrors))
            {
                result.SetData(validationErrors);
            }

            return result;
        }

        /// <summary>
        /// Retrieves the HTTP status code and error code mapping for the specified exception type
        /// </summary>
        /// <returns>A tuple containing the mapped HTTP status code and error code.</returns>
        private static (HttpStatusCode? StatusCode, string? ErrorCode) GetMapping(Exception exception)
        {
            return _mappings.TryGetValue(exception.GetType(), out var mapping)
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
            out Dictionary<string, List<string>>? errors)
        {
            errors = null;

            // Support built-in ValidationException
            if (exception is ValidationException validationEx &&
                validationEx.ValidationResult != null)
            {
                errors = [];

                foreach (var memberName in validationEx.ValidationResult.MemberNames)
                {
                    if (!errors.TryGetValue(memberName, out var messages))
                    {
                        messages = [];
                        errors[memberName] = messages;
                    }

                    if (validationEx.ValidationResult.ErrorMessage != null)
                    {
                        messages.Add(validationEx.ValidationResult.ErrorMessage);
                    }
                }

                return errors.Count > 0;
            }

            return false;
        }
    }
}
