using AspNetConventions.Core.Enums.Json;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Provides a fluent API for configuring JSON serialization rules for a specific property.
    /// </summary>
    public interface IJsonPropertyRuleBuilder
    {
        /// <summary>
        /// Configures the property to be ignored during JSON serialization.
        /// </summary>
        /// <param name="condition">
        /// The condition under which the property should be ignored.
        /// Defaults to <see cref="IgnoreCondition.Always"/>.
        /// </param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonPropertyRuleBuilder Ignore(IgnoreCondition condition = IgnoreCondition.Always);

        /// <summary>
        /// Overrides the JSON property name produced by the active naming policy.
        /// </summary>
        /// <param name="name">The exact JSON name to emit for this property.</param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonPropertyRuleBuilder Name(string name);

        /// <summary>
        /// Sets the serialization order of the property. Lower values are serialized first.
        /// </summary>
        /// <param name="order">The order value.</param>
        /// <returns>The current builder instance for chaining.</returns>
        IJsonPropertyRuleBuilder Order(int order);
    }
}
