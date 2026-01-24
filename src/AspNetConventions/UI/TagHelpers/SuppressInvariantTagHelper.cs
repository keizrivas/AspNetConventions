using System;
using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers
{
    /// <summary>
    /// Custom Label Tag Helper for kebab-case
    /// </summary>
    [HtmlTargetElement("input", Attributes = ForAttributeName)]
    public class SuppressInvariantTagHelper : ConventionTagHelper
    {
        public override int Order => 20000;

        public SuppressInvariantTagHelper(IOptions<AspNetConventionOptions> options) : base(options)
        {
            OnProcess = (context, output, parameterName, transformedParameterName) =>
            {
                var html = output.PostElement.GetContent();
                const string invariant = "name=\"__Invariant\"";

                if (output.PostElement.IsEmptyOrWhiteSpace ||
                html.IndexOf(invariant, StringComparison.Ordinal) < 0)
                {
                    return;
                }

                var original = $"value=\"{For!.Name}\"";
                var replacement = $"value=\"{transformedParameterName}\"";

                var index = html.IndexOf(original, StringComparison.Ordinal);
                if (index < 0)
                {
                    return;
                }

                var result = string.Concat(
                    html.AsSpan(0, index),
                    replacement,
                    html.AsSpan(index + original.Length)
                );

                output.PostElement.SetHtmlContent(result);
            };
        }
    }
}
