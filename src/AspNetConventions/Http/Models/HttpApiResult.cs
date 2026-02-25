using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Represents an <see cref="IResult"/> implementation for Minimal APIs
    /// that wraps an <see cref="ApiResult{TValue}"/> and writes it to the HTTP response.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of the value contained in the <see cref="ApiResult{TValue}"/>.
    /// </typeparam>
    public sealed class HttpApiResult<TValue>(ApiResult<TValue> result) : IResult, IValueHttpResult
    {
        /// <summary>
        /// Gets the result value produced by the operation.
        /// </summary>
        public object? Value => result;

        /// <summary>
        /// Executes the result operation of the current action synchronously.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var apiResult = Value as ApiResult<TValue>;
            httpContext.Response.StatusCode = (int)apiResult!.StatusCode;

            await httpContext.Response
                .WriteAsJsonAsync(apiResult)
                .ConfigureAwait(false);
        }
    }
}
