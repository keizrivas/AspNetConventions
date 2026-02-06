using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace AspNetConventions.Routing.ModelBinding
{
    /// <summary>
    /// Provides utilities for classifying model types for binding purposes.
    /// </summary>
    /// <remarks>
    /// This static class contains methods to determine whether a type should be treated as a complex
    /// bindable type versus a simple scalar type.
    /// </remarks>
    internal static class ModelTypeClassifier
    {
        /// <summary>
        /// Determines whether the specified type is a complex bindable type.
        /// </summary>
        /// <param name="type">The type to classify.</param>
        /// <returns>true if the type is a complex bindable type; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        public static bool IsComplexBindableType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            // Unwrap nullable
            type = Nullable.GetUnderlyingType(type) ?? type;

            // Scalars & enums
            if (type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid))
            {
                return false;
            }

            // Arrays
            if (type.IsArray)
            {
                return false;
            }

            // Collections
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return false;
            }

            // Must be a class or struct (records included)
            if (!type.IsClass && !type.IsValueType)
            {
                return false;
            }

            // Must have public instance properties
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(p =>
                    p.CanRead &&
                    p.GetIndexParameters().Length == 0);
        }
    }
}
