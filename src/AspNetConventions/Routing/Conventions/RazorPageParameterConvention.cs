using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options;
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
    /// <param name="options">The convention options to apply.</param>
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

            // Skip excluded pages
            if (Options.Route.RazorPages.ExcludePages.Count > 0)
            {
                var pageName = System.IO.Path.GetFileNameWithoutExtension(model.ViewEnginePath);
                if (ContainsOrdinalIgnoreCase(Options.Route.RazorPages.ExcludePages, pageName))
                {
                    return;
                }
            }

            // Skip pages inside excluded folders
            if (Options.Route.RazorPages.ExcludeFolders.Count > 0)
            {
                var segments = model.ViewEnginePath.Split('/');
                for (var i = 0; i < segments.Length - 1; i++)
                {
                    if (ContainsOrdinalIgnoreCase(Options.Route.RazorPages.ExcludeFolders, segments[i]))
                    {
                        return;
                    }
                }
            }

            // Handle page model properties ([BindProperty] class-level properties)
            if (Options.Route.RazorPages.TransformPropertyNames)
            {
                foreach (var property in model.HandlerProperties)
                {
                    var bindingContext = BindingDescriptor.GetBindingContext(property);
                    TransformBinderModelName(property, bindingContext);
                }
            }

            // Handle page handler method parameters
            if (Options.Route.RazorPages.TransformParameterNames)
            {
                foreach (var handler in model.HandlerMethods)
                {
                    foreach (var parameter in handler.Parameters)
                    {
                        var bindingContext = BindingDescriptor.GetBindingContext(parameter);
                        TransformBinderModelName(parameter, bindingContext);
                    }
                }
            }
        }

        /// <summary>
        /// Transforms the binder model name for a given parameter based on the convention options.
        /// </summary>
        /// <param name="parameter">The parameter for which to transform the binder model name.</param>
        /// <param name="bindingContext">The binding context for the parameter.</param>
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

        private static bool ContainsOrdinalIgnoreCase(HashSet<string> set, string value)
        {
            foreach (var item in set)
            {
                if (string.Equals(item, value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
