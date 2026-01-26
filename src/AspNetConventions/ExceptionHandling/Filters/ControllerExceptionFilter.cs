using System;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private readonly ILogger<ControllerExceptionFilter> _logger = logger ?? NullLogger<ControllerExceptionFilter>.Instance;
        private readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var exceptionHandling = new ExceptionHandlingManager(context.HttpContext, _options.Value, _logger);

            var (response, statusCode) = await exceptionHandling
                .BuildResponseFromExceptionAsync(context.Exception, context.Result?.GetContent())
                .ConfigureAwait(false);

            // If response is null, don't handle the exception
            if (response == null)
            {
                return;
            }

            // Return the standardized error response
            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
