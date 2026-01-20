using System;

namespace AspNetConventions.Routing.Converters
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
            return obj is WordRange other && this.Equals(other);
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
