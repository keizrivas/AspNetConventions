
using AspNetConventions.Core.Converters;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for string case conversion and manipulation.
    /// </summary>
    /// <remarks>
    /// These methods delegate to specialized case converters created by the <see cref="CaseConverterFactory"/>.
    /// They provide a convenient fluent interface for transforming strings between different naming conventions
    /// commonly used in programming and API development.
    /// </remarks>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to kebab-case (lowercase with hyphens).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The kebab-case version of the string.</returns>
        public static string ToKebabCase(this string value)
        {
            return CaseConverterFactory.CreateKebabCase().Convert(value);
        }

        /// <summary>
        /// Converts a string to snake_case (lowercase with underscores).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The snake_case version of the string.</returns>
        public static string ToSnakeCase(this string value)
        {
            return CaseConverterFactory.CreateSnakeCase().Convert(value);
        }

        /// <summary>
        /// Converts a string to camelCase (first letter lowercase, subsequent words capitalized).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The camelCase version of the string.</returns>
        public static string ToCamelCase(this string value)
        {
            return CaseConverterFactory.CreateCamelCase().Convert(value);
        }

        /// <summary>
        /// Converts a string to PascalCase (first letter of each word capitalized, no separators).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The PascalCase version of the string.</returns>
        public static string ToPascalCase(this string value)
        {
            return CaseConverterFactory.CreatePascalCase().Convert(value);
        }
    }
}
