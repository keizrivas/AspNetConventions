using System;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Providers
{
    /// <summary>
    /// Provides query string values with case-transformed parameter names according to naming conventions.
    /// </summary>
    /// <remarks>
    /// This value provider transforms incoming query parameter names using the configured case converter
    /// before looking them up in the query string.
    /// </remarks>
    public sealed class QueryValueProvider : IValueProvider
    {
        /// <summary>
        /// The original query string collection from the HTTP request.
        /// </summary>
        private readonly IQueryCollection _query;

        /// <summary>
        /// The case conversion function to apply to parameter names.
        /// </summary>
        private readonly Func<string, string> _convert;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryValueProvider"/> class.
        /// </summary>
        /// <param name="options">The AspNetConventions options.</param>
        /// <param name="query">The query string collection from the HTTP request.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public QueryValueProvider(
            IOptions<AspNetConventionOptions> options,
            IQueryCollection query)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            _query = query;
            _convert = options.Value.Route.GetCaseConverter().Convert;
        }

        /// <summary>
        /// Determines whether the query string contains any values with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to search for.</param>
        /// <returns>true if the query string contains values with the specified prefix; otherwise, false.</returns>
        public bool ContainsPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            var converted = _convert(prefix);

            // Exact prefix match (fast path)
            if (_query.ContainsKey(converted))
            {
                return true;
            }

            // Check for: prefix.*
            var prefixDot = converted + '.';

            foreach (var key in _query.Keys)
            {
                if (key.StartsWith(prefixDot, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a value from the query string using case-transformed parameter name lookup.
        /// </summary>
        /// <param name="key">The parameter name to retrieve.</param>
        /// <returns>A <see cref="ValueProviderResult"/> containing the parameter value(s), or None if not found.</returns>
        public ValueProviderResult GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return ValueProviderResult.None;
            }

            var convertedKey = _convert(key);

            // Exact match
            if (_query.TryGetValue(convertedKey, out var value))
            {
                return new(value);
            }

            // Remove first segment (supports nesting)
            var dotIndex = convertedKey.IndexOf('.', StringComparison.Ordinal);
            if (dotIndex < 0)
            {
                return ValueProviderResult.None;
            }

            var withoutPrefix = convertedKey.AsSpan(dotIndex + 1);

            // Dictionary lookup requires string
            if (_query.TryGetValue(withoutPrefix.ToString(), out value))
            {
                return new(value);
            }

            return ValueProviderResult.None;
        }
    }
}
