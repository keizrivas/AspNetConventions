using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Validation Message Tag Helper for kebab-case
    /// </summary>
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
    public class ConventionValidationMessageTagHelper : ValidationMessageTagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-for";

        public ConventionValidationMessageTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (For?.Name != null)
            {
                var kebabName = GetKebabCaseName();

                // Update data-valmsg-for attribute
                output.Attributes.SetAttribute("data-valmsg-for", kebabName);
            }
        }

        private string GetKebabCaseName()
        {
            var propertyName = For.Name;
            var metadata = For.Metadata;

            if (metadata.BinderModelName != null)
            {
                return ToKebabCase(metadata.BinderModelName);
            }

            return ToKebabCase(propertyName);
        }

        private static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                Regex.Replace(value, @"([a-z0-9])([A-Z])", "$1-$2"),
                @"([A-Z])([A-Z][a-z])",
                "$1-$2"
            ).ToLower();
        }
    }
}
