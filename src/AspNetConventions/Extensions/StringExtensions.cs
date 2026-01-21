using AspNetConventions.Common.Converters;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Extension methods for string manipulation.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to kebab-case.
        /// </summary>
        public static string ToKebabCase(this string value)
        {
            return CaseConverterFactory.CreateKebabCase().Convert(value);
        }

        /// <summary>
        /// Converts a string to snake_case.
        /// </summary>
        public static string ToSnakeCase(this string value)
        {
            return CaseConverterFactory.CreateSnakeCase().Convert(value);
        }

        /// <summary>
        /// Converts a string to camelCase.
        /// </summary>
        public static string ToCamelCase(this string value)
        {
            return CaseConverterFactory.CreateCamelCase().Convert(value);
        }

        /// <summary>
        /// Converts a string to PascalCase.
        /// </summary>
        public static string ToPascalCase(this string value)
        {
            return CaseConverterFactory.CreatePascalCase().Convert(value);
        }
    }
}
