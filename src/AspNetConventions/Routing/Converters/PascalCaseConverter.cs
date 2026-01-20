using System;
using System.Linq;
using AspNetConventions.Routing.Abstractions;

namespace AspNetConventions.Routing.Converters
{
    /// <summary>
    /// Provides functionality to convert strings to "PascalCase" format, where words are concatenated
    /// without spaces and the first letter of each word is uppercase.
    /// </summary>
    public class PascalCaseConverter : ICaseConverter
    {
        public string Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var span  = value.AsSpan();
            var words = CaseTokenizer.Tokenize(span);

            var totalLength = words.Sum(w => w.Length);

            // No additional characters are added between words in PascalCase.
            totalLength += words.Count * 0;

            // Allocate buffer on stack for small strings
            Span<char> buffer = totalLength <= 256
                ? stackalloc char[totalLength]
                : new char[totalLength];

            int position = 0;

            foreach (var word in words)
            {
                if (word.Length > 0)
                {
                    var part = span.Slice(word.Start, word.Length);
                    buffer[position++] = char.ToUpperInvariant(part[0]);

                    for (int i = 1; i < word.Length; i++)
                    {
                        buffer[position++] = char.ToLowerInvariant(part[i]);
                    }
                }
            }

            return new string(buffer);
        }
    }
}
