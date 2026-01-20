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
            if (options.Route.CaseStyle == CasingStyle.Custom && options.Route.CustomCaseConverter == null)
            {
                return ValidateOptionsResult.Fail("Route.CustomConverter must be provided when Route.CaseStyle is \"Custom\".");
            }

            if (options.Response.Style == ResponseStyle.Custom && options.Response.CustomResponseBuilder == null)
            {
               return ValidateOptionsResult.Fail("Response.CustomResponseBuilder must be provided when Response.Style is \"Custom\".");
            }

            if (options.Json.CaseStyle == CasingStyle.Custom && options.Json.CustomCaseConverter == null)
            {
               return ValidateOptionsResult.Fail("Json.CustomConverter must be provided when Json.CaseStyle is \"Custom\".");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
