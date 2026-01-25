using System;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Core.Enums;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.ExceptionHandling
{
    /// <summary>
    /// Provides helper methods for exception handling.
    /// </summary>
    internal class ExceptionHandlingHelpers(AspNetConventionOptions options, RequestDescriptor requestDescriptor, ILogger? logger = null) : ResponseAdapter(options)
    {
        private readonly IErrorResponseBuilder _builder
            = options.Response.GetErrorResponseBuilder(options);

        public ExceptionHandlingHelpers(AspNetConventionOptions options, HttpContext httpContext, ILogger? logger = null)
            : this(options, httpContext.ToRequestDescriptor(), logger)
        {
            logger ??= httpContext.GetLogger<ExceptionDescriptor>();
        }

        public override bool IsWrappedResponse(object? data)
        {
            return _builder.IsWrappedResponse(data);
        }

        public bool ShouldHandleResponse(object? data = null)
        {
            return Options.ExceptionHandling.IsEnabled && !IsWrappedResponse(data) && requestDescriptor.EndpointType == EndpointType.MvcController;
        }

        public Task TryHandleAsync(Exception exception)
        {
            var hook = Options.ExceptionHandling.Hooks.TryHandleAsync;
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
            var hooks = Options.ExceptionHandling.Hooks;

            var shouldHandle = await (
                hooks.ShouldHandleAsync?.Invoke(exception, requestDescriptor)
                ?? Task.FromResult(true)).ConfigureAwait(false);

            // Check if we should handle this exception
            if (!shouldHandle)
            {
                return (null, requestDescriptor.StatusCode);
            }

            // Create exception context
            var statusCode = Options.Response.ErrorResponse.DefaultStatusCode;
            var requestResult = new RequestResult(statusCode);

            // Get mapper
            var mapper = Options.ExceptionHandling.GetExceptionMapper(
                exception,
                requestDescriptor);

            // Allow override exception mapper
            if (hooks.BeforeMappingAsync != null)
            {
                mapper = await hooks.BeforeMappingAsync(
                    mapper,
                    requestResult,
                    requestDescriptor
                ).ConfigureAwait(false);
            }

            // Map exception
            var exceptionDescriptor = mapper.MapException(
                exception,
                requestDescriptor);

            // Allow override exception envelope
            if (hooks.AfterMappingAsync != null)
            {
                exceptionDescriptor = await hooks.AfterMappingAsync(
                    exceptionDescriptor,
                    mapper,
                    requestResult,
                    requestDescriptor
                ).ConfigureAwait(false);
            }

            // Use defaults message and error code if not set
            if ((exceptionDescriptor.Type == null || exceptionDescriptor.Message == null) &&
                exceptionDescriptor.StatusCode == statusCode)
            {
                exceptionDescriptor.Type ??= Options.Response.ErrorResponse.DefaultErrorType;
                exceptionDescriptor.Message ??= Options.Response.ErrorResponse.DefaultErrorMessage;
            }

            return await WrapResponseAsync(exceptionDescriptor, requestResult).ConfigureAwait(false);
        }

        //public Task<(object? Response, HttpStatusCode StatusCode)> WrapResponseAsync(
        //    ExceptionDescriptor2 exceptionDescriptor,
        //    Exception? exception = null)
        //{
        //    return WrapResponseAsync(exceptionDescriptor, exception);
        //}

        public async Task<(object? Response, HttpStatusCode StatusCode)> WrapResponseAsync(
            ExceptionDescriptor exceptionDescriptor,
            RequestResult? requestResult = null,
            Exception? exception = null)
        {

            var hooks = Options.Response.Hooks;

            // Log exception if needed
            if (exceptionDescriptor.ShouldLog)
            {
                var logMessage = $"Exception occurred: StatusCode={exceptionDescriptor.StatusCode}, ErrorCode={exceptionDescriptor.Type}, Message={exceptionDescriptor.Message}";

                if (exception != null)
                {
                    logger.LogError(exception, logMessage);
                }
                else
                {
                    logger.LogError(logMessage);
                }
            }

            requestResult ??= new RequestResult(
                data: exceptionDescriptor.Data,
                message: exceptionDescriptor.Message,
                statusCode: (HttpStatusCode)exceptionDescriptor.StatusCode!,
                type: exceptionDescriptor.Type);

            //// Set metadata if needed
            //if (Options.Response.IncludeMetadata)
            //{
            //    exceptionDescriptor.SetMetadata((Metadata)requestDescriptor);

            //    // Include stack trace
            //    if (exceptionDescriptor != null && ValidateCondition(Options.ExceptionHandling.IncludeStackTrace, requestDescriptor))
            //    {
            //        exceptionDescriptor.Metadata!.StackTrace = exceptionDescriptor.StackTrace;
            //    }

            //    // Include exception details
            //    if (exceptionDescriptor != null && ValidateCondition(Options.ExceptionHandling.IncludeExceptionDetails, requestDescriptor))
            //    {
            //        exceptionDescriptor.Metadata!.ExceptionType = exceptionDescriptor.ExceptionType;
            //    }
            //}

            // Determine if we should wrap the response
            var shouldWrap = await hooks.ShouldWrapResponseAsync.InvokeAsync<bool?>(exceptionDescriptor, requestDescriptor)
                .ConfigureAwait(false) ?? true;

            if (!shouldWrap)
            {
                return (null, (HttpStatusCode)exceptionDescriptor.StatusCode);
            }

            // Invoke hooks before wrapping
            await hooks.BeforeResponseWrapAsync.InvokeAsync(exceptionDescriptor, requestDescriptor)
                .ConfigureAwait(false);

            // Wrap response
            var wrappedResponse = _builder.BuildResponse(
                requestResult,
                exception,
                requestDescriptor);

            // Invoke hooks after wrapping
            await hooks.AfterResponseWrapAsync.InvokeAsync(wrappedResponse, exceptionDescriptor, requestDescriptor)
                .ConfigureAwait(false);

            return (wrappedResponse, (HttpStatusCode)exceptionDescriptor.StatusCode);
        }

        private static bool ValidateCondition(bool? condition, RequestDescriptor requestDescriptor)
        {
            return (condition == null && requestDescriptor.IsDevelopment) || condition == true;
        }
    }
}
