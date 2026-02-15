using System;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers
{
    /// <summary>
    /// Custom Label Tag Helper for kebab-case
    /// </summary>
    [HtmlTargetElement("input", Attributes = ForAttributeName)]
    public class InvariantTagHelper : ConventionTagHelper
    {
        public override int Order => 20000;

        /// The name attribute used to identify the invariant input element in the output HTML.
        public const string NameAttribute = "__Invariant";

        public InvariantTagHelper(IOptions<AspNetConventionOptions> options) : base(options)
        {
            OnProcess = (context, output, parameterName, transformedParameterName) =>
            {
                var html = output.PostElement.GetContent();
                const string invariant = $"name=\"{NameAttribute}\"";

                // If the "_invariant" name is not present in the output, 
                // there's no need to modify it.
                if (output.PostElement.IsEmptyOrWhiteSpace ||
                html.IndexOf(invariant, StringComparison.Ordinal) < 0)
                {
                    return;
                }

                var original = $"value=\"{ForModel!.Name}\"";
                var replacement = $"value=\"{transformedParameterName}\"";

                var index = html.IndexOf(original, StringComparison.Ordinal);
                if (index < 0)
                {
                    return;
                }

                // Replace the original value with the transformed parameter 
                // name in the output HTML.
                var result = string.Concat(
                    html.AsSpan(0, index),
                    replacement,
                    html.AsSpan(index + original.Length)
                );

                // Update the PostElement content with the modified HTML.
                output.PostElement.SetHtmlContent(result);
            };
        }
    }
}
