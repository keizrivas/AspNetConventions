using System;
using System.Net;
using AspNetConventions.Core.Enums;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for HTTP status code classification and analysis.
    /// </summary>
    internal static class HttpStatusCodeExtensions
    {
        /// <summary>
        /// Categorizes an HTTP status code into its corresponding type classification.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to categorize.</param>
        /// <returns>A <see cref="HttpStatusCodeType"/> representing the category of the status code.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the status code is outside the valid HTTP status code range (100-599).</exception>
        internal static HttpStatusCodeType GetHttpStatusCodeType(this HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            return code switch
            {
                >= 100 and <= 199 => HttpStatusCodeType.Informational,
                >= 200 and <= 299 => HttpStatusCodeType.Success,
                >= 300 and <= 399 => HttpStatusCodeType.Redirection,
                >= 400 and <= 499 => HttpStatusCodeType.ClientError,
                >= 500 and <= 599 => HttpStatusCodeType.ServerError,
                _ => throw new InvalidOperationException("Invalid HTTP status code.")
            };
        }
    }
}
