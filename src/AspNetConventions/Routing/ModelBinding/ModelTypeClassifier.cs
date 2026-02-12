using System;
using System.Collections;
using System.IO;
using System.IO.Pipelines;
using System.Security.Claims;
using System.Threading;

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
        /// <remarks>
        /// A complex type must have a public default constructor and public writable properties to bind.
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?#complex-types">Complex Types</see>.
        /// </remarks>
        public static bool IsComplexType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            // Scalars & enums
            if (type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeOnly) ||
                type == typeof(DateOnly) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
                type == typeof(ClaimsPrincipal) ||
                type == typeof(CancellationToken) ||
                type == typeof(Stream) ||
                type == typeof(PipeReader))
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

            // Check if the underlying type in a nullable is valid
            if (Nullable.GetUnderlyingType(type) is { } nullableType)
            {
                return IsComplexType(nullableType);
            }

            // Must be a class or struct (records included)
            return type.IsClass || type.IsValueType;
        }
    }
}
