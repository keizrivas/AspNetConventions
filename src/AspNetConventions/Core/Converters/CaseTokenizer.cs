using System;
using System.Collections.Generic;

namespace AspNetConventions.Core.Converters
{
    /// <summary>
    /// Provides methods for splitting a character span into word ranges based on casing and common word separators.
    /// </summary>
    /// <remarks>This class is intended for internal use in scenarios where text needs to be tokenized into
    /// words according to casing conventions and separators like underscores or hyphens.
    /// </remarks>
    public static class CaseTokenizer
    {
        /// <summary>
        /// Splits <paramref name="span"/> into word ranges.
        /// </summary>
        /// <param name="span">The character span to tokenize.</param>
        /// <param name="numberBoundaries">
        /// When <see langword="true"/>, transitions between letter and digit characters are treated as
        /// word boundaries.
        /// Defaults to <see langword="false"/> to preserve the existing behavior of all built-in converters.
        /// </param>
        public static IReadOnlyList<WordRange> Tokenize(ReadOnlySpan<char> span, bool numberBoundaries = false)
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

                if (i > start)
                {
                    char prevChar = span[i - 1];

                    // Uppercase boundary inside a lowercase word
                    if (char.IsUpper(currentChar) && char.IsLower(prevChar))
                    {
                        words.Add(new WordRange(start, i - start));
                        start = i;
                    }
                    // Letter/digit boundary (only when opt-in)
                    else if (numberBoundaries &&
                             (char.IsDigit(currentChar) != char.IsDigit(prevChar)))
                    {
                        words.Add(new WordRange(start, i - start));
                        start = i;
                    }
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
