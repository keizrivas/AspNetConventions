using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
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
        IOptions<AspNetConventionOptions> options) : ConventionOptions(options), IAsyncExceptionFilter
    {
        private readonly ILogger<ControllerExceptionFilter> _logger = logger ?? NullLogger<ControllerExceptionFilter>.Instance;

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            CreateOptionSnapshot();
            if (!Options.ExceptionHandling.IsEnabled)
            {
                return;
            }

            var exceptionHandlingFactory = new ExceptionHandlingFactory(context.HttpContext, Options, _logger);

            var (response, statusCode) = await exceptionHandlingFactory
                .BuildResponseFromExceptionAsync(context.Exception, (context.Result as ObjectResult)?.Value)
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
