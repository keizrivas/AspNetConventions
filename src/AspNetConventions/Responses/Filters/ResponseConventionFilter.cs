using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Responses.Filters
{
    internal sealed class ResponseConventionFilter(IOptions<AspNetConventionOptions> options, ILogger<ResponseConventionFilter> logger) : ConventionOptions(options), IAsyncResultFilter
    {
        private readonly ILogger<ResponseConventionFilter> _logger = logger ?? NullLogger<ResponseConventionFilter>.Instance;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            CreateOptionSnapshot();

            if (!Options.Response.IsEnabled ||
                context.Result is not ObjectResult objectResult)
            {
                await next().ConfigureAwait(false);
                return;
            }

            var payload = objectResult.Value;

            // Create http request context and attach to HttpContext items
            var requestDescriptor = context.HttpContext.GetRequestDescriptor();

            requestDescriptor.SetStatusCode(
                (HttpStatusCode)(objectResult.StatusCode
                    ?? context.HttpContext.Response.StatusCode));

            var responseFactory = new ResponseFactory(Options, requestDescriptor, _logger);

            // Check if the response is already wrapped
            if (responseFactory.IsWrappedResponse(payload))
            {
                await next().ConfigureAwait(false);
                return;
            }

            // Build the wrapped response
            var (response, statusCode) = await responseFactory.BuildResponseAsync(payload)
                .ConfigureAwait(false);

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)statusCode
            };

            objectResult.Value = response;
            objectResult.StatusCode = (int)statusCode;

            await next().ConfigureAwait(false);
        }
    }
}
