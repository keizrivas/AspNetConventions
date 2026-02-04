using System;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetConventions.Responses.Filters
{
    public sealed class ResponseConventionEndpointFilter(AspNetConventionOptions options) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {

            ArgumentNullException.ThrowIfNull(next, nameof(next));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            // Result
            object? result;

            // Response manager
            var responseManager = new ResponseManager(options, context.HttpContext);

            result = await next(context).ConfigureAwait(false);
            if (context.HttpContext.Response.HasStarted)
            {
                return result;
            }

            // Don't wrap terminal results
            if (result is IResult iResult && IsTerminalResult(iResult))
            {
                return result;
            }

            // Already wrapped
            var requestResult = responseManager.GetRequestResultFromContent(result);
            if (responseManager.IsWrappedResponse(requestResult))
            {
                return result;
            }

            var (response, statusCode) = await responseManager
                .BuildResponseAsync(requestResult)
                .ConfigureAwait(false);

            return Results.Json(response, statusCode: (int)statusCode);
        }

        private static bool IsTerminalResult(IResult result)
        {
            return result is
                IFileHttpResult or
                RedirectHttpResult or
                ChallengeHttpResult or
                ForbidHttpResult or
                SignInHttpResult or
                SignOutHttpResult;
        }
    }
}
