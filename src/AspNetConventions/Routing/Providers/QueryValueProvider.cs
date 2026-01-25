using System;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Providers
{
    public sealed class QueryValueProvider : IValueProvider
    {
        private readonly IQueryCollection _query;
        private readonly Func<string, string> _convert;

        public QueryValueProvider(
            IOptions<AspNetConventionOptions> options,
            IQueryCollection query)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            _query = query;
            _convert = options.Value.Route.GetCaseConverter().Convert;
        }

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

            // Remove fisrt segment (supports nesting)
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
