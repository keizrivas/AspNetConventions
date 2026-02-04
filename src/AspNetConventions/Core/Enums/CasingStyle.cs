namespace AspNetConventions.Core.Enums
{
    /// <summary>
    /// Specifies the casing style options for formatting text identifiers.
    /// </summary>
    /// <remarks>Use this enumeration to indicate how strings should be cased. The available styles include
    /// "camelCase", "PascalCase", "kebab-case", and "snake_case" for consistent naming conventions across your application.
    /// </remarks>
    public enum CasingStyle
    {
        /// <summary>
        /// camelCase style: First word is lowercase, subsequent words start with uppercase.
        /// </summary>
        CamelCase,

        /// <summary>
        /// PascalCase style: All words start with uppercase.
        /// </summary>
        PascalCase,

        /// <summary>
        /// kebab-case style: Words are separated by hyphens and all lowercase.
        /// </summary>
        KebabCase,

        /// <summary>
        /// snake_case style: Words are separated by underscores and all lowercase.
        /// </summary>
        SnakeCase,
    }
}
