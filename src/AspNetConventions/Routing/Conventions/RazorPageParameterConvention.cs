using AspNetConventions.Configuration;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Routing.ModelBinding;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Applies naming conventions to Razor Page handler parameters.
    /// </summary>
    internal sealed class RazorPageParameterConvention(IOptions<AspNetConventionOptions> options) : ConventionOptions(options), IPageApplicationModelConvention
    {
        public void Apply(PageApplicationModel model)
        {
            // Setup options
            CreateOptionSnapshot();

            if (!Options.Route.IsEnabled || !Options.Route.RazorPages.IsEnabled)
            {
                return;
            }

            // Handle page model properties
            foreach (var property in model.HandlerProperties)
            {
                var bindingContext = BindingDescriptor.GetBindingContext(property);
                TransformBinderModelName(property, bindingContext);
                //TransformProperty(property);
            }

            // Handle page model parameters
            foreach (var handler in model.HandlerMethods)
            {
                foreach (var parameter in handler.Parameters)
                {
                    var bindingContext = BindingDescriptor.GetBindingContext(parameter);
                    TransformBinderModelName(parameter, bindingContext);
                    //TransformParameter(parameter);
                }
            }
        }

        private void TransformBinderModelName(ParameterModelBase parameter, BindingContext bindingContext)
        {
            if (!bindingContext.IsBindable)
            {
                return;
            }

            // Skip if should not set binder model name
            if (bindingContext.MetadataKind == ModelMetadataKind.Parameter &&
                !BindingDescriptor.ShouldSetBinderModelName(bindingContext, out var name))
            {
                return;
            }
            else
            {
                name = bindingContext.ModelName;
            }

            var caseConverter = Options.Route.GetCaseConverter();
            var transformed = caseConverter.Convert(name);

            parameter.BindingInfo ??= new BindingInfo();
            parameter.BindingInfo.BinderModelName = transformed;
        }
    }
}
