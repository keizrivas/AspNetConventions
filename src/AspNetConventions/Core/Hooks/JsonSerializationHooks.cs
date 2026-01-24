using System;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace AspNetConventions.Core.Hooks
{
    public class JsonSerializationHooks
    {
        public delegate Task<bool> ShouldSerializePropertyCallbackAsync(Exception exception);

        public ShouldSerializePropertyCallbackAsync? ShouldSerializePropertyAsync { get; set; }
    }
}
