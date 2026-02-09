using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetConventions.ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Http;

namespace AspNetConventions.ExceptionHandling.Abstractions
{
    /// <summary>
    /// Defines a contract for writing standardized error responses when exceptions occur during request processing.
    /// </summary>
    internal interface IExceptionResponseWriter
    {
        /// <summary>
        /// Set the JSON serializer options to the current <see cref="IExceptionResponseWriter"/> instance to be used when writing error responses.
        /// </summary>
        /// <param name="jsonSerializerOptions">The JSON serializer options to use for formatting error responses.</param>
        /// <returns>The current instance of <see cref="IExceptionResponseWriter"/> with the updated serializer options.</returns>
        ExceptionResponseWriter WithSerializerOptions(JsonSerializerOptions jsonSerializerOptions);

        /// <summary>
        /// Writes a standardized error response to the HTTP context based on the provided exception.
        /// </summary>
        /// <param name="httpContext">The HTTP context to write the response to.</param>
        /// <param name="exception">The exception that occurred during request processing.</param>
        /// <param name="cancellationToken">A cancellation token to observe while writing the response.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public ValueTask WriteResponseAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken);
    }
}
