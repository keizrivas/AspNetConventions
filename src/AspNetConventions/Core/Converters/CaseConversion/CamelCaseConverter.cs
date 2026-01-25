using System;
using System.Linq;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Core.Converters.CaseConversion
{
    /// <summary>
    /// Provides functionality to convert strings to "camelCase" format, where words are concatenated without spaces and
    /// the first letter of the first word is lowercase while the first letters of subsequent words are uppercase.
    /// </summary>
    public class CamelCaseConverter : ICaseConverter
    {
        public string Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var span = value.AsSpan();
            var words = CaseTokenizer.Tokenize(span);

            var totalLength = words.Sum(w => w.Length);

            // Allocate buffer on stack for small strings
            Span<char> buffer = totalLength <= 256
                ? stackalloc char[totalLength]
                : new char[totalLength];

            int position = 0;
            bool first = true;

            foreach (var w in words)
            {
                if (w.Length == 0)
                {
                    continue;
                }

                var part = span.Slice(w.Start, w.Length);

                if (first)
                {
                    // lower first word
                    buffer[position++] = char.ToLowerInvariant(part[0]);
                    for (int i = 1; i < w.Length; i++)
                    {
                        buffer[position++] = char.ToLowerInvariant(part[i]);
                    }
                    first = false;
                }
                else
                {
                    // upper first letter
                    buffer[position++] = char.ToUpperInvariant(part[0]);
                    for (int i = 1; i < w.Length; i++)
                    {
                        buffer[position++] = char.ToLowerInvariant(part[i]);
                    }
                }
            }

            return new string(buffer);
        }
    }
}
