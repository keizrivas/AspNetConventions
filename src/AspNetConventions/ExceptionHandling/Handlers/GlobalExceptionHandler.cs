using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetConventions.ExceptionHandling.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AspNetConventions.ExceptionHandling.Handlers
{
    /// <summary>
    /// Exception handler that provides standardized error responses.
    /// </summary>
    /// <param name="exceptionResponseWriter">The service responsible for writing exception responses.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options to use when formatting error responses.</param>
    /// <remarks>
    /// This handler integrates with AspNetConventions to provide consistent error response formatting
    /// </remarks>
    internal sealed class GlobalExceptionHandler(
        IExceptionResponseWriter exceptionResponseWriter,
        JsonSerializerOptions jsonSerializerOptions) : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the specified exception for the Minimal API request context.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the request.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>true if the exception was handled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> or <paramref name="exception"/> is null.</exception>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            await exceptionResponseWriter
                .WithSerializerOptions(jsonSerializerOptions)
                .WriteResponseAsync(httpContext, exception, cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
    }
}
