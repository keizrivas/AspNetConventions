using System;
using System.Text.Json;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="JsonSerializerOptions"/> configuration and manipulation.
    /// </summary>
    internal static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Applies configuration settings from one <see cref="JsonSerializerOptions"/> instance to another.
        /// </summary>
        /// <param name="target">The target <see cref="JsonSerializerOptions"/> to apply settings to.</param>
        /// <param name="source">The source <see cref="JsonSerializerOptions"/> to copy settings from.</param>
        /// <returns>The same <see cref="JsonSerializerOptions"/> instance as <paramref name="target"/> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="source"/> is null.</exception>
        /// <remarks>
        /// This method performs a deep copy of all JSON serialization settings.
        /// </remarks>
        internal static JsonSerializerOptions ApplyFrom(
            this JsonSerializerOptions target,
            JsonSerializerOptions source)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(source);

            // Scalar options
            target.PropertyNamingPolicy = source.PropertyNamingPolicy;
            target.DictionaryKeyPolicy = source.DictionaryKeyPolicy;
            target.PropertyNameCaseInsensitive = source.PropertyNameCaseInsensitive;
            target.DefaultIgnoreCondition = source.DefaultIgnoreCondition;
            target.IgnoreReadOnlyFields = source.IgnoreReadOnlyFields;
            target.IgnoreReadOnlyProperties = source.IgnoreReadOnlyProperties;
            target.IncludeFields = source.IncludeFields;
            target.WriteIndented = source.WriteIndented;
            target.AllowTrailingCommas = source.AllowTrailingCommas;
            target.MaxDepth = source.MaxDepth;
            target.NumberHandling = source.NumberHandling;
            target.ReadCommentHandling = source.ReadCommentHandling;
            target.UnknownTypeHandling = source.UnknownTypeHandling;
            target.PreferredObjectCreationHandling = source.PreferredObjectCreationHandling;
            target.Encoder = source.Encoder;
            target.ReferenceHandler = source.ReferenceHandler;

            // TypeInfoResolver / chain
            if (source.TypeInfoResolverChain is { Count: > 0 })
            {
                target.TypeInfoResolverChain.Clear();
                foreach (var resolver in source.TypeInfoResolverChain)
                {
                    target.TypeInfoResolverChain.Add(resolver);
                }
            }
            else
            {
                target.TypeInfoResolver = source.TypeInfoResolver;
            }

            // Converters: append without removing MVC defaults
            foreach (var converter in source.Converters)
            {
                if (!target.Converters.Contains(converter))
                {
                    target.Converters.Add(converter);
                }
            }

            return target;
        }
    }
}
