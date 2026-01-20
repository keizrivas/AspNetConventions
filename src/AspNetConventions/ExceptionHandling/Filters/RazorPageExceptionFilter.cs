using System;
using System.Threading.Tasks;
using AspNetConventions.Configuration;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetConventions.ExceptionHandling.Filters
{
    /// <summary>
    /// Provides an ASP.NET Core razor pages filter that handles unhandled exceptions and generates standardized error
    /// responses according to configured conventions.
    /// </summary>
    /// <param name="logger">The logger used to record exception details and diagnostic information.</param>
    /// <param name="options">The options that configure exception handling behavior and response formatting.</param>
    internal sealed class RazorPageExceptionFilter(
        IOptions<AspNetConventionOptions> options,
        ILogger<RazorPageExceptionFilter> logger) : IAsyncPageFilter
    {
        private readonly ILogger<RazorPageExceptionFilter> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        public async Task OnPageHandlerExecutionAsync(
            PageHandlerExecutingContext context,
            PageHandlerExecutionDelegate next)
        {
            if (!_options.Value.ExceptionHandling.IsEnabled)
            {
                await next().ConfigureAwait(false);
                return;
            }

            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // If accepting JSON, return JSON error response
                if (context.HttpContext.AcceptsJson())
                {
                    var helper = new ExceptionHandlingHelpers(_options.Value, context.HttpContext, _logger);
                    var (response, statusCode) = await helper.BuildExceptionResponseAsync(exception)
                        .ConfigureAwait(false);

                    context.Result = new JsonResult(response)
                    {
                        StatusCode = (int)statusCode
                    };
                }
                else
                {
                    // Let Razor Pages error handling continue
                    _logger.LogError(exception, "Unhandled exception in Razor Page handler");
                    throw;
                }
            }
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
            => Task.CompletedTask;
    }
}
