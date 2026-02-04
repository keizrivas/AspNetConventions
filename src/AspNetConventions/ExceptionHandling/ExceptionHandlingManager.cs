using System;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetConventions.ExceptionHandling
{
    /// <summary>
    /// Provides helper methods for exception handling in AspNetConventions.
    /// </summary>
    /// <remarks>
    /// This class handles the complete exception processing pipeline, including exception mapping,
    /// logging, and response building. It integrates with configurable hooks and mappers to provide
    /// flexible exception handling strategies.
    /// </remarks>
    internal class ExceptionHandlingManager(RequestDescriptor requestDescriptor, AspNetConventionOptions options, ILogger? logger = null)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingManager"/> class using an HttpContext.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <param name="logger">Optional logger for diagnostic information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> or <paramref name="options"/> is null.</exception>
        public ExceptionHandlingManager(HttpContext httpContext, AspNetConventionOptions options, ILogger? logger = null)
            : this(httpContext.GetRequestDescriptor(), options, logger)
        {
        }

        private readonly ILogger _logger = logger ?? NullLogger<ExceptionHandlingManager>.Instance;
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Builds an exception descriptor from request context and exception.
        /// </summary>
        /// <param name="exception">The exception to process.</param>
        /// <returns>An <see cref="ExceptionDescriptor"/> if the exception should be handled; otherwise, null.</returns>
        /// <exception cref="InvalidOperationException">Thrown when critical services are missing or configuration is invalid.</exception>
        internal async Task<ExceptionDescriptor?> BuildExceptionDescriptorAsync(Exception exception)
        {
            var hooks = _options.ExceptionHandling.Hooks;

            var shouldHandle = await hooks.ShouldHandleAsync.InvokeAsync<bool?>(
                exception,
                requestDescriptor)
                .ConfigureAwait(false) ?? true;

            // Check if we should handle this exception
            if (!shouldHandle)
            {
                return null;
            }

            // Get mapper
            var mapper = _options.ExceptionHandling.GetExceptionMapper(
                exception,
                requestDescriptor);

            // Allow override exception mapper
            if (hooks.BeforeMappingAsync != null)
            {
                mapper = await hooks.BeforeMappingAsync(
                    mapper,
                    requestDescriptor
                ).ConfigureAwait(false);
            }

            // Map exception
            var exceptionDescriptor = mapper.MapException(
                exception,
                requestDescriptor);

            // Set original exception
            exceptionDescriptor.Exception = exception;

            // Use default data if not set
            exceptionDescriptor.Type ??= options.Response.ErrorResponse.DefaultErrorType;
            exceptionDescriptor.Message ??= options.Response.ErrorResponse.DefaultErrorMessage;
            exceptionDescriptor.StatusCode ??= options.Response.ErrorResponse.DefaultStatusCode;

            // Allow override exception envelope
            if (hooks.AfterMappingAsync != null)
            {
                exceptionDescriptor = await hooks.AfterMappingAsync(
                    exceptionDescriptor,
                    mapper,
                    requestDescriptor
                ).ConfigureAwait(false);
            }

            // Log exception if needed
            if (exceptionDescriptor.ShouldLog)
            {
                var logMessage = $"Exception occurred: StatusCode={exceptionDescriptor.StatusCode}, ErrorType={exceptionDescriptor.Type}, Message={exceptionDescriptor.Message}";

                if (exception != null)
                {
                    _logger.Log(exceptionDescriptor.LogLevel, exception, logMessage);
                }
                else
                {
                    _logger.Log(exceptionDescriptor.LogLevel, logMessage);
                }
            }

            return exceptionDescriptor;
        }

        /// <summary>
        /// Determines if the specified exception should be handled by the exception handling system.
        /// </summary>
        /// <param name="exception">The exception to evaluate.</param>
        /// <returns>true if the exception should be handled; otherwise, false.</returns>
        internal bool ShouldHandleException(Exception exception)
        {
            return _options.ExceptionHandling.IsEnabled &&
                !_options.ExceptionHandling.ExcludeException.TryGetValue(exception.GetType(), out _) &&
                !_options.ExceptionHandling.ExcludeStatusCodes.TryGetValue(requestDescriptor.StatusCode, out _);
        }

        /// <summary>
        /// Builds a response from an exception, applying exception handling conventions.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="content">Optional existing content that may be wrapped.</param>
        /// <returns>A tuple containing the response object and HTTP status code.</returns>
        /// <exception cref="InvalidOperationException">Thrown when response building fails due to configuration issues.</exception>
        internal async Task<(object? Response, HttpStatusCode StatusCode)> BuildResponseFromExceptionAsync(
            Exception exception, object? content)
        {
            // Check if should handle the exception
            if (!ShouldHandleException(exception))
            {
                return (null, requestDescriptor.StatusCode);
            }

            // Invoke global handle hook
            await options.ExceptionHandling.Hooks.TryHandleAsync
                .InvokeAsync(exception)
                .ConfigureAwait(false);

            var responseManager = new ResponseManager(options, requestDescriptor, _logger);

            // Check if response is already wrapped
            if (responseManager.IsWrappedResponse(content))
            {
                return (null, requestDescriptor.StatusCode);
            }

            // Build exception descriptor
            if (content is not ExceptionDescriptor)
            {
                content = await BuildExceptionDescriptorAsync(exception)
                    .ConfigureAwait(false);
            }

            // Build the exception response
            return await responseManager.BuildResponseAsync(content)
                .ConfigureAwait(false);
        }
    }
}
