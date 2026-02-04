using System;
using System.Linq;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Core.Converters.CaseConversion
{
    /// <summary>
    /// Provides functionality to convert strings to "kebab-case" format, where words are lowercase and
    /// separated by hyphens.
    /// </summary>
    public class KebabCaseConverter : ICaseConverter
    {
        /// <summary>
        /// Converts the input string to kebab-case format.
        /// </summary>
        /// <param name="value">The string to convert. Can be null, empty, or contain various separators like spaces, hyphens, or underscores.</param>
        /// <returns>The kebab-case version of the input string. Returns an empty string if the input is null or whitespace.</returns>
        public string Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var span = value.AsSpan();
            var words = CaseTokenizer.Tokenize(span);

            // Calculate total length needed for the output string (including separator)
            int totalLength = words.Sum(w => w.Length) + (words.Count - 1);

            // Allocate buffer on stack for small strings
            Span<char> buffer = totalLength <= 256
                ? stackalloc char[totalLength]
                : new char[totalLength];

            int position = 0;
            for (int wi = 0; wi < words.Count; wi++)
            {
                var word = words[wi];
                var part = span.Slice(word.Start, word.Length);
                for (int i = 0; i < word.Length; i++)
                {
                    buffer[position++] = char.ToLowerInvariant(part[i]);
                }

                if (wi < words.Count - 1)
                {
                    buffer[position++] = '-';
                }
            }

            return new string(buffer);
        }
    }
}
