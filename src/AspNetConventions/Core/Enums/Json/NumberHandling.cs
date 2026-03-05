namespace AspNetConventions.Core.Enums.Json
{
    /// <summary>
    /// Specifies how numbers are handled during JSON serialization and deserialization.
    /// </summary>
    public enum NumberHandling
    {
        /// <summary>
        /// Numbers are handled strictly according to JSON specification.
        /// </summary>
        Strict,

        /// <summary>
        /// Allows reading numbers from JSON strings during deserialization.
        /// </summary>
        AllowReadingFromString,

        /// <summary>
        /// Writes numbers as strings during serialization.
        /// </summary>
        WriteAsString,

        /// <summary>
        /// Allows reading and writing named floating-point literals (e.g., NaN, Infinity).
        /// </summary>
        AllowNamedFloatingPointLiterals
    }
}
