using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Common.Enums;
using AspNetConventions.Configuration;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.ExceptionHandling
{
    /// <summary>
    /// Provides helper methods for exception handling.
    /// </summary>
    internal class ExceptionHandlingHelpers(AspNetConventionOptions options, RequestDescriptor requestDescriptor, ILogger? logger = null) : ResponseAdapter(options)
    {
        private readonly AspNetConventionOptions _options = options;
        private readonly IExceptionResponseBuilder _builder = options.ExceptionHandling.GetResponseBuilder(options);

        public ExceptionHandlingHelpers(AspNetConventionOptions options, HttpContext httpContext, ILogger? logger = null)
            : this(options, httpContext.ToRequestDescriptor(), logger)
        {
           logger ??= httpContext.GetLogger<ExceptionDescriptor>();
        }

        public override bool IsWrappedResponse(object? data)
        {
            return _builder.IsWrappedResponse(data);
        }

        public bool ShouldHandleResponse(object? data =  null)
        {
            return _options.ExceptionHandling.IsEnabled && !IsWrappedResponse(data) && requestDescriptor.EndpointType == EndpointType.MvcController;
        }

        public Task TryHandleAsync(Exception exception)
        {
            var hook = _options.ExceptionHandling.Hooks.TryHandleAsync;
            if (hook != null)
            {
                return hook.Invoke(exception);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds an exception response from request context and exception.
        /// </summary>
        public async Task<(object? Response, HttpStatusCode StatusCode)> BuildExceptionResponseAsync(Exception exception)
        {
            var hooks = _options.ExceptionHandling.Hooks;

            var shouldHandle = await (
                hooks.ShouldHandleAsync?.Invoke(exception, requestDescriptor)
                ?? Task.FromResult(true)).ConfigureAwait(false);

            // Check if we should handle this exception
            if (!shouldHandle)
            {
                return (null, requestDescriptor.StatusCode);
            }

            // Create exception context
            var statusCode = _options.ExceptionHandling.DefaultStatusCode;
            var exceptionDescriptor = new ExceptionDescriptor(exception, statusCode);

            // Get mapper
            var mapper = _options.ExceptionHandling.GetExceptionMapper(
                exceptionDescriptor,
                requestDescriptor);

            // Allow override exception mapper
            if (hooks.BeforeMappingAsync != null)
            {
                mapper = await hooks.BeforeMappingAsync(
                    mapper,
                    exceptionDescriptor,
                    requestDescriptor
                ).ConfigureAwait(false);
            }

            // Map exception
            var exceptionEnvelope = mapper.MapException(
                exceptionDescriptor,
                requestDescriptor);

            // Allow override exception envelope
            if (hooks.AfterMappingAsync != null)
            {
                exceptionEnvelope = await hooks.AfterMappingAsync(
                    exceptionEnvelope,
                    mapper,
                    exceptionDescriptor,
                    requestDescriptor
                ).ConfigureAwait(false);
            }

            // Use defaults message and error code if not set
            if ((exceptionEnvelope.ErrorCode == null || exceptionEnvelope.Message == null) &&
                exceptionEnvelope.StatusCode == statusCode)
            {
                exceptionEnvelope.ErrorCode ??= _options.ExceptionHandling.DefaultErrorCode;
                exceptionEnvelope.Message   ??= _options.ExceptionHandling.DefaultErrorMessage;
            }

            return await WrapResponseAsync(exceptionEnvelope, exceptionDescriptor).ConfigureAwait(false);
        }

        public Task<(object? Response, HttpStatusCode StatusCode)> WrapResponseAsync(
            ExceptionEnvelope exceptionEnvelope,
            Exception? exception = null)
        {
            return WrapResponseAsync(exceptionEnvelope, exception != null
                ? new ExceptionDescriptor(exception, exceptionEnvelope.StatusCode)
                : null);
        }

        public async Task<(object? Response, HttpStatusCode StatusCode)> WrapResponseAsync(
            ExceptionEnvelope exceptionEnvelope,
            ExceptionDescriptor? exceptionDescriptor = null)
        {

            var hooks = _options.Response.Hooks;

            // Log exception if needed
            exceptionEnvelope.LogException(logger, exceptionDescriptor?.Exception);

            // Set metadata if needed
            if (_options.Response.IncludeMetadata)
            {
                exceptionEnvelope.SetMetadata((Metadata)requestDescriptor);

                // Include stack trace
                if (exceptionDescriptor != null && ValidateCondition(_options.ExceptionHandling.IncludeStackTrace, requestDescriptor))
                {
                    exceptionEnvelope.Metadata!.StackTrace = exceptionDescriptor.StackTrace?.ToList();
                }

                // Include exception details
                if (exceptionDescriptor != null && ValidateCondition(_options.ExceptionHandling.IncludeExceptionDetails, requestDescriptor))
                {
                    exceptionEnvelope.Metadata!.ExceptionType = exceptionDescriptor.ExceptionType;
                }
            }

            // Determine if we should wrap the response
            var shouldWrap = await hooks.ShouldWrapResponseAsync.InvokeAsync<bool?>(exceptionEnvelope, requestDescriptor)
                .ConfigureAwait(false) ?? true;

            if (!shouldWrap)
            {
                return (null, exceptionEnvelope.StatusCode);
            }

            // Invoke hooks before wrapping
            await hooks.BeforeResponseWrapAsync.InvokeAsync(exceptionEnvelope, requestDescriptor)
                .ConfigureAwait(false);

            // Wrap response
            var wrappedResponse = _builder.BuildResponse(
                exceptionEnvelope,
                exceptionDescriptor,
                requestDescriptor);

            // Invoke hooks after wrapping
            await hooks.AfterResponseWrapAsync.InvokeAsync(wrappedResponse, exceptionEnvelope, requestDescriptor)
                .ConfigureAwait(false);

            return (wrappedResponse, exceptionEnvelope.StatusCode);
        }

        private static bool ValidateCondition(bool? condition, RequestDescriptor requestDescriptor)
        {
            return (condition == null && requestDescriptor.IsDevelopment) || condition == true;
        }
    }
}
