using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.TagHelpers
{
    /// <summary>
    /// Custom Textarea Tag Helper that renders kebab-case names and IDs
    /// </summary>
    [HtmlTargetElement("textarea", Attributes = ForAttributeName)]
    public class TextAreaTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
