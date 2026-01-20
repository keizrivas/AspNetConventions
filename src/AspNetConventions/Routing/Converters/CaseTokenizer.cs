using System;
using System.Collections.Generic;

namespace AspNetConventions.Routing.Converters
{
    /// <summary>
    /// Provides methods for splitting a character span into word ranges based on casing and common word separators.
    /// </summary>
    /// <remarks>This class is intended for internal use in scenarios where text needs to be tokenized into
    /// words according to casing conventions and separators like underscores or hyphens.
    /// </remarks>
    public static class CaseTokenizer
    {
        public static IReadOnlyList<WordRange> Tokenize(ReadOnlySpan<char> span)
        {
            int start = 0;
            var words = new List<WordRange>(8);

            for (int i = 0; i < span.Length; i++)
            {
                char currentChar = span[i];

                // Separator boundary
                if (currentChar == '_' || currentChar == '-')
                {
                    if (i > start)
                    {
                        words.Add(new WordRange(start, i - start));
                    }

                    start = i + 1;
                    continue;
                }

                // Uppercase boundary inside a lowercase word
                if (i > start && char.IsUpper(currentChar) && char.IsLower(span[i - 1]))
                {
                    words.Add(new WordRange(start, i - start));
                    start = i;
                }
            }

            // Add the final word if any
            if (start < span.Length)
            {
                words.Add(new WordRange(start, span.Length - start));
            }

            return words;
        }
    }
}
