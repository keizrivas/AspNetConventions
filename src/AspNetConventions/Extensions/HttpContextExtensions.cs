using System;
using AspNetConventions.Http;
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
        internal static RequestDescriptor ToRequestDescriptor(this HttpContext httpContext)
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
        /// Determines if the request accepts JSON responses.
        /// </summary>
        internal static bool AcceptsJson(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            return httpContext.Request.Headers.Accept.ToString()
                .Contains(ContentTypes.Json, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the request accepts HTML responses.
        /// </summary>
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

        internal static int? GetNumericParameter(this HttpContext httpContext, string parameterName)
        {
            var raw = httpContext.GetQueryValue(parameterName);
            return int.TryParse(raw, out var value) && value > 0 ? value : null;
        }

        internal static int GetNumericParameter(this HttpContext httpContext, string parameterName, int defaultValue = 0)
        {
            return httpContext.GetNumericParameter(parameterName) ?? defaultValue;
        }

        internal static ILogger GetLogger<T>(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService(typeof(ILogger<T>)) as ILogger
                ?? throw new InvalidOperationException("Logger service is not available.");
        }
    }
}
