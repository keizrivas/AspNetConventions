using System;
using System.Text.Json.Serialization;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Provides a fluent API for registering JSON serialization rules for multiple types.
    /// </summary>
    public interface IJsonTypesConfigurationBuilder
    {
        /// <summary>
        /// Registers JSON serialization rules for a specific closed type.
        /// </summary>
        /// <typeparam name="T">The type to configure.</typeparam>
        /// <param name="configure">A delegate that receives a type rule builder.</param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonTypesConfigurationBuilder Type<T>(Action<IJsonTypeRuleBuilder<T>> configure);

        /// <summary>
        /// Registers JSON serialization rules for an open generic type, applying them to all
        /// closed variants at runtime.
        /// </summary>
        /// <typeparam name="T">
        /// A closed instantiation of the open generic type to use as a property-selection template.
        /// Must itself be a generic type.
        /// </typeparam>
        /// <param name="configure">A delegate that receives a type rule builder.</param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonTypesConfigurationBuilder OpenGenericType<T>(Action<IJsonTypeRuleBuilder<T>> configure);

        /// <summary>
        /// Applies an ignore condition to every property on <typeparamref name="T"/> and all
        /// subclasses when they are serialized.
        /// </summary>
        /// <typeparam name="T">The type whose properties should be ignored.</typeparam>
        /// <param name="condition">
        /// The ignore condition to apply. Defaults to <see cref="JsonIgnoreCondition.Always"/>.
        /// </param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonTypesConfigurationBuilder IgnoreType<T>(
            JsonIgnoreCondition condition = JsonIgnoreCondition.Always);

        /// <summary>
        /// Applies an ignore condition to any property whose JSON name matches
        /// <paramref name="name"/>, across all types.
        /// </summary>
        /// <remarks>
        /// The match is case-insensitive and is only applied when no more specific
        /// per-type rule exists for the same property.
        /// </remarks>
        /// <param name="name">The JSON property name to match (case-insensitive).</param>
        /// <param name="condition">
        /// The ignore condition to apply. Defaults to <see cref="JsonIgnoreCondition.Always"/>.
        /// </param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonTypesConfigurationBuilder IgnorePropertyName(
            string name,
            JsonIgnoreCondition condition = JsonIgnoreCondition.Always);
    }
}
