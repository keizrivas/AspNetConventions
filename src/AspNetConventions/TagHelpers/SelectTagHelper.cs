using System.Text.RegularExpressions;
using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.TagHelpers
{
    /// <summary>
    /// Custom Select Tag Helper that renders kebab-case names and IDs
    /// </summary>
    [HtmlTargetElement("select", Attributes = ForAttributeName)]
    public class SelectTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
