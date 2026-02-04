namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for converting strings between different naming conventions.
    /// </summary>
    public interface ICaseConverter
    {
        /// <summary>
        /// Converts a string from one naming convention to another.
        /// </summary>
        /// <param name="value">The input string to convert. Can be null, empty, or contain various separators like spaces, hyphens, or underscores.</param>
        /// <returns>The converted string in the target naming convention. Returns an empty string if the input is null or whitespace.</returns>
        string Convert(string value);
    }
}
