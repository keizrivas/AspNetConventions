using System.Threading.Tasks;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetConventions.ExceptionHandling.Filters
{
    internal class ExceptionStatusCodeResultFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var content = context.Result.GetContent();

            // Check if the result contains an ExceptionEnvelope and set the status code
            if (content is ExceptionDescriptor exceptionDescriptor && exceptionDescriptor.StatusCode != default)
            {
                var statusCode = (int)exceptionDescriptor.StatusCode;
                switch (context.Result)
                {
                    case ObjectResult objectResult:
                        objectResult.StatusCode = statusCode;
                        break;

                    case ContentResult contentResult:
                        contentResult.StatusCode = statusCode;
                        break;

                    case JsonResult jsonResult:
                        jsonResult.StatusCode = statusCode;
                        break;

                    default:
                        context.HttpContext.Response.StatusCode = (int)exceptionDescriptor.StatusCode;
                        break;
                }
            }

            await next().ConfigureAwait(false);
        }
    }
}
