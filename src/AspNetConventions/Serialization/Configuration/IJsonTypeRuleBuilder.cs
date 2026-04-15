using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Provides a fluent API for configuring JSON serialization rules for the properties of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type whose properties are being configured.</typeparam>
    public interface IJsonTypeRuleBuilder<T>
    {
        /// <summary>
        /// Selects a property on <typeparamref name="T"/> and returns a builder for configuring its rules.
        /// </summary>
        /// <typeparam name="TProp">The type of the selected property.</typeparam>
        /// <param name="selector">
        /// A strongly-typed expression selecting the property.
        /// </param>
        /// <returns>A property rule builder for further configuration.</returns>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
            Justification = "Property is an intentional, well-known API name in this domain (mirrors EF Core).")]
        IJsonPropertyRuleBuilder Property<TProp>(Expression<Func<T, TProp>> selector);
    }
}
