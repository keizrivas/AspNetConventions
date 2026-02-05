using System;
using System.Threading.Tasks;

namespace AspNetConventions.Core.Hooks
{
    public class JsonSerializationHooks : ICloneable
    {
        public delegate Task<bool> ShouldSerializePropertyCallbackAsync(Exception exception);

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
