using System;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AspNetConventions.Common.Hooks
{
    public class JsonSerializationHooks
    {
        public Func<JsonIgnoreCondition, string, JsonPropertyInfo, bool>? ShouldSerialize { get; set; }
    }
}
