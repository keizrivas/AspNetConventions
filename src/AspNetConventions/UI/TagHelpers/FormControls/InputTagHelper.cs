using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Input Tag Helper that targets "input" elements with the "asp-for" attribute and 
    /// ensures that the input element is correctly associated with the transformed parameter name.
    /// </summary>
    /// <param name="options">The convention options, injected via dependency injection.</param>
    [HtmlTargetElement("input", Attributes = ForAttributeName)]
    public class InputTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
