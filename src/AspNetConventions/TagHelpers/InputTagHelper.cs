using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.TagHelpers
{
    /// <summary>
    /// Custom Input Tag Helper that renders kebab-case names and IDs
    /// </summary>
    [HtmlTargetElement("input", Attributes = ForAttributeName)]
    public class InputTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
