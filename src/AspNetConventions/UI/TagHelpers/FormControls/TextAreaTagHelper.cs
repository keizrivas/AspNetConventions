using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers.FormControls
{
    /// <summary>
    /// Custom Textarea Tag Helper that targets "textarea" elements with the "asp-for" attribute and 
    /// ensures that the textarea element is correctly associated with the transformed parameter name.
    /// </summary>
    /// <param name="options">The convention options, injected via dependency injection.</param>
    [HtmlTargetElement("textarea", Attributes = ForAttributeName)]
    public class TextAreaTagHelper(IOptions<AspNetConventionOptions> options) : ConventionTagHelper(options)
    {

    }
}
