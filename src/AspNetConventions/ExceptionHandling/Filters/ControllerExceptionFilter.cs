using System;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetConventions.ExceptionHandling.Filters
{
    /// <summary>
    /// Provides an ASP.NET Core MVC exception filter that handles unhandled exceptions and generates standardized error
    /// responses according to configured conventions.
    /// </summary>
    /// <param name="logger">The logger used to record exception details and diagnostic information.</param>
    /// <param name="options">The options that configure exception handling behavior and response formatting.</param>
    internal sealed class ControllerExceptionFilter(
        ILogger<ControllerExceptionFilter> logger,
        IOptions<AspNetConventionOptions> options) : IAsyncExceptionFilter
    {
        private readonly ILogger<ControllerExceptionFilter> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;
            var helper = new ExceptionHandlingHelpers(_options.Value, context.HttpContext, _logger);

            // Check if exception handling is enabled and if response is already wrapped
            if (!context.HttpContext.AcceptsJson() || !helper.ShouldHandleResponse(context.Result?.GetContent()))
            {
                return;
            }

            // Invoke global handle hook
            await helper.TryHandleAsync(exception).ConfigureAwait(false);

            // Build the exception response
            var (response, statusCode) = await helper.BuildExceptionResponseAsync(exception)
                .ConfigureAwait(false);

            // If response is null, don't handle the exception
            if (response == null)
            {
                return;
            }

            // Set the result to return the standardized error response
            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
