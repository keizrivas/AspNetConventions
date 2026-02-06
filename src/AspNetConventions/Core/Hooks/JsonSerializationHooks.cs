using System;
using System.Threading.Tasks;

namespace AspNetConventions.Core.Hooks
{
    /// <summary>
    /// Provides hooks for customizing JSON serialization behavior by allowing injection of user-defined
    /// logic at stages of the serialization process.
    /// </summary>
    /// <remarks>
    /// Use this class to register delegates that are invoked to determine whether specific properties
    /// should be serialized into JSON output.
    /// </remarks>
    public class JsonSerializationHooks : ICloneable
    {
        /// <summary>
        /// Represents an asynchronous callback method to determine whether a property should be serialized.
        /// </summary>
        /// <param name="exception">The exception being serialized.</param>
        /// <returns>A task that returns true if the property should be serialized; otherwise, false.</returns>
        public delegate Task<bool> ShouldSerializePropertyCallbackAsync(Exception exception);

        /// <summary>
        /// Gets or sets the asynchronous callback to determine whether a property should be serialized.
        /// </summary>
        /// <value>A callback that returns false to skip serialization of the specified property.</value>
        public ShouldSerializePropertyCallbackAsync? ShouldSerializePropertyAsync { get; set; }

        /// <summary>
        /// Creates a deep clone of <see cref="JsonSerializationHooks"/> instance.
        /// </summary>
        /// <returns>A new <see cref="JsonSerializationHooks"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            return new JsonSerializationHooks
            {
                ShouldSerializePropertyAsync = ShouldSerializePropertyAsync,
            };
        }
    }
}
