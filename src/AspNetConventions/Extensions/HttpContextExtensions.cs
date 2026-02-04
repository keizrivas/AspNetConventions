using System;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for HttpContext.
    /// </summary>
    internal static class HttpContextExtensions
    {
        private const string RequestDescriptorKey = "RequestDescriptor";

        /// <summary>
        /// Gets or creates an RequestDescriptor from HttpContext.
        /// </summary>
        internal static RequestDescriptor GetRequestDescriptor(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (httpContext.Items.TryGetValue(RequestDescriptorKey, out var existing) &&
                existing is RequestDescriptor context)
            {
                return context;
            }

            var newContext = new RequestDescriptor(httpContext);
            httpContext.Items[RequestDescriptorKey] = newContext;
            return newContext;
        }

        /// <summary>
        /// Determines if the request accepts JSON responses by checking the Accept header.
        /// </summary>
        /// <param name="httpContext">The HTTP context to check.</param>
        /// <returns>true if the Accept header contains "application/json"; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        internal static bool AcceptsJson(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            return httpContext.Request.Headers.Accept.ToString()
                .Contains(ContentTypes.Json, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the request accepts HTML responses by checking the Accept header.
        /// </summary>
        /// <param name="httpContext">The HTTP context to check.</param>
        /// <returns>true if the Accept header contains "text/html"; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        internal static bool AcceptsHtml(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            return httpContext.Request.Headers.Accept.ToString()
                .Contains(ContentTypes.Html, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the HTTP request contains a query parameter with the specified key, using a case-insensitive
        /// key comparison.
        /// </summary>
        /// <param name="httpContext">The HTTP context representing the current request.</param>
        /// <param name="parameterName">The name of the query string parameter.</param>
        /// <returns>true if the query string contains a parameter with the specified key; otherwise, false.</returns>
        internal static bool HasQueryParam(this HttpContext httpContext, string parameterName)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var normalizedExpected = parameterName.NormalizeQueryKey();
            foreach (var key in httpContext.Request.Query.Keys)
            {
                if (key.NormalizeQueryKey() == normalizedExpected)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the value of the specified query string parameter from the HTTP request, using a case-insensitive
        /// key comparison.
        /// </summary>
        /// <param name="httpContext">The HTTP context representing the current request.</param>
        /// <param name="parameterName">The name of the query string parameter.</param>
        /// <returns>The value of the specified query string parameter if found; otherwise, null.</returns>
        internal static string? GetQueryValue(this HttpContext httpContext, string parameterName)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var normalizedExpected = parameterName.NormalizeQueryKey();
            foreach (var value in httpContext.Request.Query)
            {
                if (value.Key.NormalizeQueryKey() == normalizedExpected)
                {
                    return value.Value.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a normalized version of the specified query key by converting it to uppercase and removing hyphens
        /// and underscores.
        /// </summary>
        /// <param name="key">The query key to normalize.</param>
        /// <returns>A normalized string representing the query key in uppercase with all hyphens and underscores removed.</returns>
        internal static string NormalizeQueryKey(this string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            // Normalize
            return key.ToUpperInvariant()
                .Replace("-", string.Empty, StringComparison.InvariantCulture)
                .Replace("_", string.Empty, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Retrieves a numeric query parameter value from the HTTP request.
        /// </summary>
        /// <param name="httpContext">The HTTP context representing the current request.</param>
        /// <param name="parameterName">The name of the query string parameter.</param>
        /// <returns>The numeric value if parsing succeeds and the value is positive; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        /// <remarks>This method uses case-insensitive parameter name matching and only returns positive integers.</remarks>
        internal static int? GetNumericParameter(this HttpContext httpContext, string parameterName)
        {
            var raw = httpContext.GetQueryValue(parameterName);
            return int.TryParse(raw, out var value) && value > 0 ? value : null;
        }

        /// <summary>
        /// Retrieves a numeric query parameter value from the HTTP request, returning a default value if not found or invalid.
        /// </summary>
        /// <param name="httpContext">The HTTP context representing the current request.</param>
        /// <param name="parameterName">The name of the query string parameter.</param>
        /// <param name="defaultValue">The default value to return if the parameter is not found or invalid. Defaults to 0.</param>
        /// <returns>The numeric value if parsing succeeds and the value is positive; otherwise, the default value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        /// <remarks>This method uses case-insensitive parameter name matching and only accepts positive integers.</remarks>
        internal static int GetNumericParameter(this HttpContext httpContext, string parameterName, int defaultValue = 0)
        {
            return httpContext.GetNumericParameter(parameterName) ?? defaultValue;
        }

        /// <summary>
        /// Retrieves an ILogger instance for the specified type from the HTTP context's service provider.
        /// </summary>
        /// <typeparam name="T">The type to create a logger for.</typeparam>
        /// <param name="httpContext">The HTTP context containing the service provider.</param>
        /// <returns>An ILogger instance for the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the logging service is not available.</exception>
        internal static ILogger GetLogger<T>(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService(typeof(ILogger<T>)) as ILogger
                ?? throw new InvalidOperationException("Logger service is not available.");
        }
    }
}
