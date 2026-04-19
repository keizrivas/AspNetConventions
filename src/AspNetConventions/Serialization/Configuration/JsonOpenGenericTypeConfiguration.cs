using System;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Base class for class-based JSON serialization configuration for an open generic type.
    /// The rules apply to every closed variant of the generic at runtime.
    /// </summary>
    /// <typeparam name="T">
    /// A closed instantiation of the open generic type to use as a property-selection template.
    /// Must itself be a generic type.
    /// </typeparam>
    public abstract class JsonOpenGenericTypeConfiguration<T> : JsonTypeConfigurationBase
    {
        /// <summary>
        /// Configures JSON serialization rules for the open generic type represented by <typeparamref name="T"/>.
        /// </summary>
        /// <param name="rule">The type rule builder.</param>
        public abstract void Configure(IJsonTypeRuleBuilder<T> rule);

        internal override void ApplyCore(JsonTypesConfigurationBuilder builder)
        {
            if (!typeof(T).IsGenericType)
            {
                throw new InvalidOperationException(
                    $"'{typeof(T).Name}' is not a generic type. " +
                    $"Use {nameof(JsonTypeConfiguration<T>)}<T> for non-generic types.");
            }

            builder.OpenGenericType<T>(Configure);
        }
    }
}
