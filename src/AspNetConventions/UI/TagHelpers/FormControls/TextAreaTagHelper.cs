using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Textarea Tag Helper that renders kebab-case names and IDs
    /// </summary>
    [HtmlTargetElement("textarea", Attributes = ForAttributeName)]
    public class TextAreaTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
