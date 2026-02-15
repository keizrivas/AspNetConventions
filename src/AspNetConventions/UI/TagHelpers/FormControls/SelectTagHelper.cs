using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Select Tag Helper that targets "select" elements with the "asp-for" attribute and 
    /// ensures that the select element is correctly associated with the transformed parameter name.
    /// </summary>
    /// <param name="options">The convention options, injected via dependency injection.</param>
    [HtmlTargetElement("select", Attributes = ForAttributeName)]
    public class SelectTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
