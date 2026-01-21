using System;
using AspNetConventions.Common.Abstractions;

namespace AspNetConventions.Serialization.Policies
{
    /// <summary>
    /// A JSON naming policy that uses a case converter.
    /// </summary>
    internal sealed class CustomJsonNamingPolicy(ICaseConverter converter) : System.Text.Json.JsonNamingPolicy
    {
        private readonly ICaseConverter _converter = converter ?? throw new ArgumentNullException(nameof(converter));

        public override string ConvertName(string name) => _converter.Convert(name);
    }
}
