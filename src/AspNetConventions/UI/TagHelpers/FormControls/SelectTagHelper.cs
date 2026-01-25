using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Select Tag Helper that renders kebab-case names and IDs
    /// </summary>
    [HtmlTargetElement("select", Attributes = ForAttributeName)]
    public class SelectTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
