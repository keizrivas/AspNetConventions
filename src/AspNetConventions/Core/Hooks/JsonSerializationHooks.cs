using System;
using System.Collections.Generic;

namespace AspNetConventions.Core.Hooks
{
    /// <summary>
    /// Provides hooks for customizing JSON serialization behavior at configuration time and at serialization time.
    /// </summary>
    public class JsonSerializationHooks : ICloneable
    {
        /// <summary>
        /// Called once per type during configuration.
        /// Return <see langword="false"/> to skip all <b>AspNetConventions</b> rule processing for
        /// this type; the serializer will still use its own defaults for it.
        /// </summary>
        public Func<Type, bool>? ShouldSerializeType { get; set; }

        /// <summary>
        /// Called once per property during configuration.
        /// Return a non-<see langword="null"/> string to override the serialized JSON property name.
        /// Return <see langword="null"/> to keep the name produced by the active naming policy and
        /// any <c>ConfigureTypes</c> rules.
        /// </summary>
        public Func<string, Type, string?>? ResolvePropertyName { get; set; }

        /// <summary>
        /// Called on every serialization for every property.
        /// Return <see langword="false"/> to suppress the property from the output.
        /// </summary>
        public Func<object, object?, string, Type, bool>? ShouldSerializeProperty { get; set; }

        /// <summary>
        /// Called once per type after its metadata has been fully resolved.
        /// Use this hook for startup-time logging or diagnostics.
        /// </summary>
        public Action<Type, IReadOnlyList<string>>? OnTypeResolved { get; set; }

        public object Clone()
        {
            return new JsonSerializationHooks
            {
                ShouldSerializeType = ShouldSerializeType,
                ResolvePropertyName = ResolvePropertyName,
                ShouldSerializeProperty = ShouldSerializeProperty,
                OnTypeResolved = OnTypeResolved,
            };
        }
    }
}
