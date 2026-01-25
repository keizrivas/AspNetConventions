using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Label Tag Helper for kebab-case
    /// </summary>
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class LabelTagHelper : ConventionTagHelper
    {
        public LabelTagHelper(IOptions<AspNetConventionOptions> options) : base(options)
        {
            OnProcess = (context, output, parameterName, transformedParameterName) =>
            {
                // Set "for" attribute
                //output.Attributes.RemoveAll("for");
                //output.Attributes.Add("for", parameterName);
            };
        }
    }
}
