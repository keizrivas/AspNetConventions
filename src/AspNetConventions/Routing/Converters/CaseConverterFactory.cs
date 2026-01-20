using System;
using AspNetConventions.Common.Enums;
using AspNetConventions.Routing.Abstractions;

namespace AspNetConventions.Routing.Converters
{
    /// <summary>
    /// Provides factory methods for creating case converter instances for various casing styles.
    /// </summary>
    /// <remarks>This class offers static methods to obtain converters for common casing styles such as
    /// kebab-case, snake_case, camelCase, and PascalCase. All methods return shared instances and are thread-safe. Use
    /// these methods to obtain an appropriate converter for transforming strings between different casing
    /// conventions.</remarks>
    public static class CaseConverterFactory
    {
        private static readonly Lazy<KebabCaseConverter> KebabCase =
            new(() => new KebabCaseConverter());

        private static readonly Lazy<SnakeCaseConverter> SnakeCase =
            new(() => new SnakeCaseConverter());

        private static readonly Lazy<CamelCaseConverter> CamelCase =
            new(() => new CamelCaseConverter());

        private static readonly Lazy<PascalCaseConverter> PascalCase =
            new(() => new PascalCaseConverter());

        /// <summary>
        /// Creates a kebab-case converter.
        /// </summary>
        public static ICaseConverter CreateKebabCase() => KebabCase.Value;

        /// <summary>
        /// Creates a snake_case converter.
        /// </summary>
        public static ICaseConverter CreateSnakeCase() => SnakeCase.Value;

        /// <summary>
        /// Creates a camelCase converter.
        /// </summary>
        public static ICaseConverter CreateCamelCase() => CamelCase.Value;

        /// <summary>
        /// Creates a PascalCase converter.
        /// </summary>
        public static ICaseConverter CreatePascalCase() => PascalCase.Value;

        /// <summary>
        /// Creates a case converter based on the specified style.
        /// </summary>
        public static ICaseConverter Create(CasingStyle style)
        {
            return style switch
            {
                CasingStyle.KebabCase  => CreateKebabCase(),
                CasingStyle.SnakeCase  => CreateSnakeCase(),
                CasingStyle.CamelCase  => CreateCamelCase(),
                CasingStyle.PascalCase => CreatePascalCase(),
                _ => CreateKebabCase()
            };
        }
    }
}
