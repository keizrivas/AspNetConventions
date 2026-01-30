using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetConventions.Responses
{
    public sealed class ApiEnvelopeEndpointFilter(AspNetConventionOptions options) : IEndpointFilter
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

            try
            {
                result = await next(context).ConfigureAwait(false);
            }
            catch
            {
                // Let exception middleware handle it
                throw;
            }

            if (result is IResult iResult)
            {
                // Don't wrap terminal results
                if (IsTerminalResult(iResult))
                {
                    return result;
                }
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
                SignInHttpResult or
                SignOutHttpResult or
                ChallengeHttpResult or
                ForbidHttpResult;
        }
    }
}
