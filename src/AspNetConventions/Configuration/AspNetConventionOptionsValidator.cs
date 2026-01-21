using AspNetConventions.Common.Enums;
using AspNetConventions.ResponseFormatting.Enums;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Configuration
{
    internal sealed class AspNetConventionOptionsValidator
        : IValidateOptions<AspNetConventionOptions>
    {
        public ValidateOptionsResult Validate(
            string? name,
            AspNetConventionOptions options)
        {
            return ValidateOptionsResult.Success;
        }
    }
}
