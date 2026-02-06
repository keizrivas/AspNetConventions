using System;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Serialization.Policies
{
    /// <summary>
    /// A JSON naming policy that uses a case converter.
    /// </summary>
    /// <param name="converter">The case converter to apply to property names.</param>
    internal sealed class CustomJsonNamingPolicy(ICaseConverter converter) : System.Text.Json.JsonNamingPolicy
    {
        private readonly ICaseConverter _converter = converter ?? throw new ArgumentNullException(nameof(converter));

        /// <summary>
        /// Converts a property name using the configured case converter.
        /// </summary>
        /// <param name="name">The property name to convert.</param>
        /// <returns>The converted property name.</returns>
        /// <remarks>
        /// This method delegates the actual conversion to the injected ICaseConverter
        /// </remarks>
        public override string ConvertName(string name) => _converter.Convert(name);
    }
}
