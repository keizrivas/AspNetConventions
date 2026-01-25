using System;
using System.Threading.Tasks;

namespace AspNetConventions.Core.Hooks
{
    public class JsonSerializationHooks: ICloneable
    {
        public delegate Task<bool> ShouldSerializePropertyCallbackAsync(Exception exception);

        public ShouldSerializePropertyCallbackAsync? ShouldSerializePropertyAsync { get; set; }

        public object Clone()
        {
            return new JsonSerializationHooks
            {
                ShouldSerializePropertyAsync = ShouldSerializePropertyAsync,
            };
        }
    }
}
