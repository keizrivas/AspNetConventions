using System;

namespace AspNetConventions.Core.Converters
{
    /// <summary>
    /// Represents a range of word positions within a text, defined by a starting index and length.
    /// </summary>
    public readonly struct WordRange : IEquatable<WordRange>
    {
        /// <summary>
        /// Gets the zero-based starting index of the range.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// The number of words included in the range. Must be non-negative.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WordRange"/> struct with the specified starting index and length.
        /// </summary> <param name="start">The zero-based starting index of the range.</param>
        /// <param name="length">The number of words included in the range. Must be non-negative.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is negative.</exception>
        public WordRange(int start, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
            }

            Start = start;
            Length = length;
        }

        public override bool Equals(object? obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return obj is WordRange other && Equals(other);
        }

        public bool Equals(WordRange other)
        {
            return Start == other.Start && Length == other.Length;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, Length);
        }

        public static bool operator ==(WordRange left, WordRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WordRange left, WordRange right)
        {
            return !(left == right);
        }
    }
}
