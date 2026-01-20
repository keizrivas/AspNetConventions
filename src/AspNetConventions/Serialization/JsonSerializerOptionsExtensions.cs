using System;
using System.Text.Json;

namespace AspNetConventions.Serialization
{
    /// <summary>
    /// Extension methods for JsonSerializerOptions.
    /// </summary>
    public static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Applies options from one JsonSerializerOptions to another.
        /// </summary>
        public static JsonSerializerOptions ApplyOptions(
            this JsonSerializerOptions target,
            JsonSerializerOptions source)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(source);

            target.PropertyNamingPolicy = source.PropertyNamingPolicy;
            target.PropertyNameCaseInsensitive = source.PropertyNameCaseInsensitive;
            target.DefaultIgnoreCondition = source.DefaultIgnoreCondition;
            target.WriteIndented = source.WriteIndented;
            target.AllowTrailingCommas = source.AllowTrailingCommas;
            target.MaxDepth = source.MaxDepth;
            target.NumberHandling = source.NumberHandling;
            target.TypeInfoResolver = source.TypeInfoResolver;

            target.Converters.Clear();
            foreach (var converter in source.Converters)
            {
                target.Converters.Add(converter);
            }

            return target;
        }
    }
}
