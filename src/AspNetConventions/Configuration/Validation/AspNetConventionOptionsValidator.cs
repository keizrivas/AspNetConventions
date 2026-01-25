using AspNetConventions.Configuration.Options;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Configuration.Validation
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
