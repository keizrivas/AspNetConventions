using System;
using System.Net;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AspNetConventions.Responses.Filters
{
    /// <summary>
    /// An endpoint filter that applies AspNetConventions response formatting to Minimal API endpoints.
    /// </summary>
    /// <param name="options">The convention options for response formatting.</param>
    /// <remarks>
    /// This filter intercepts endpoint execution results and applies standardized response formatting
    /// according to the configured AspNetConventions options.
    /// </remarks>
    public sealed class ResponseConventionEndpointFilter(AspNetConventionOptions options) : IEndpointFilter
    {
        /// <summary>
        /// Asynchronously invokes the endpoint filter to apply response conventions.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context containing HTTP context and arguments.</param>
        /// <param name="next">The delegate to invoke the next filter in the chain or the endpoint itself.</param>
        /// <returns>The filtered result, typically a JSON response with standardized formatting.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="next"/> is null.</exception>
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var result = await next(context).ConfigureAwait(false);
            if (context.HttpContext.Response.HasStarted
                || result is not IResult iResult
                || IsTerminalResult(iResult))
            {
                return result;
            }

            // Unwrap nested results
            if (iResult is INestedHttpResult nestedResult)
            {
                iResult = nestedResult.Result;
            }

            // Set status code from result if available
            var requestDescriptor = context.HttpContext.GetRequestDescriptor();

            requestDescriptor.SetStatusCode((HttpStatusCode)(
                result is IStatusCodeHttpResult { StatusCode: int status }
                    ? status
                    : context.HttpContext.Response.StatusCode));

            var responseFactory = new ResponseFactory(options, requestDescriptor);
            var apiResult = responseFactory.GetRequestResultFromContent(
                (iResult as IValueHttpResult)?.Value);

            // Already wrapped
            if (responseFactory.IsWrappedResponse(apiResult))
            {
                return result;
            }

            var (response, statusCode) = await responseFactory
                .BuildResponseAsync(apiResult)
                .ConfigureAwait(false);

            return Results.Json(response, statusCode: (int)statusCode);
        }

        /// <summary>
        /// Determines if the specified result is a terminal result that should not be wrapped.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns>true if the result is terminal; otherwise, false.</returns>
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
