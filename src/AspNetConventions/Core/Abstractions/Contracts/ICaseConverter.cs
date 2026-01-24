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
        /// <param name="value">The input string to convert (typically PascalCase).</param>
        /// <returns>The converted string in the target naming convention.</returns>
        string Convert(string value);
    }
}
