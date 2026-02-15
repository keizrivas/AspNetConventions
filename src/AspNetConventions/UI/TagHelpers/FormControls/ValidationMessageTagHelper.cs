using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom ValidationMessage Tag Helper that targets "span" elements with the "asp-validation-for" 
    /// attribute and ensures that the validation message is correctly associated with the transformed parameter name. 
    /// </summary>
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
    public class ValidationMessageTagHelper : ConventionTagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-for";
        private const string DataValidationForAttributeName = "data-valmsg-for";

        [HtmlAttributeName(ValidationForAttributeName)]
        public override required ModelExpression ForModel { get; set; } = default!;

        public ValidationMessageTagHelper(IOptions<AspNetConventionOptions> options) : base(options)
        {
            OnProcess = (context, output, parameterName, transformedParameterName) =>
            {
                // Remove any existing data-valmsg-for attribute to prevent conflicts with 
                // the default behavior of the ValidationMessageTagHelper and add our transformed attribute.
                output.Attributes.RemoveAll(DataValidationForAttributeName);
                output.Attributes.Add(DataValidationForAttributeName, transformedParameterName);
            };
        }
    }
}
