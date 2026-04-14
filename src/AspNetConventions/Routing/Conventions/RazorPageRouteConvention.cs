using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Applies naming conventions to Razor Page routes.
    /// </summary>
    /// <param name="options">The convention options to apply.</param>
    internal sealed class RazorPageRouteConvention(IOptions<AspNetConventionOptions> options) : ConventionOptions(options), IPageRouteModelConvention
    {
        public void Apply(PageRouteModel pageModel)
        {
            // Setup options
            CreateOptionSnapshot();

            // Check if route transformation is enabled
            if (!Options.Route.IsEnabled || !Options.Route.RazorPages.IsEnabled)
            {
                return;
            }

            // Skip excluded pages
            if (Options.Route.RazorPages.ExcludePages.Count > 0)
            {
                var pageName = System.IO.Path.GetFileNameWithoutExtension(pageModel.ViewEnginePath);
                if (ContainsOrdinalIgnoreCase(Options.Route.RazorPages.ExcludePages, pageName))
                {
                    return;
                }
            }

            // Skip pages inside excluded folders
            if (Options.Route.RazorPages.ExcludeFolders.Count > 0)
            {
                var segments = pageModel.ViewEnginePath.Split('/');
                // All segments except the last are folder names
                for (var i = 0; i < segments.Length - 1; i++)
                {
                    if (ContainsOrdinalIgnoreCase(Options.Route.RazorPages.ExcludeFolders, segments[i]))
                    {
                        return;
                    }
                }
            }

            var caseConverter = Options.Route.GetCaseConverter();
            foreach (var selector in pageModel.Selectors)
            {
                // Check if action selector has a route template
                var template = selector.AttributeRouteModel?.Template;
                if (selector.AttributeRouteModel == null || string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                // Skip templates that exceed the configured maximum length
                if (template.Length > Options.Route.MaxRouteTemplateLength)
                {
                    continue;
                }

                var modelContext = RouteModelContext.FromRazorPage(selector, pageModel);

                // Determine if route should be transformed
                var shouldTransformRoute = Options.Route.Hooks.ShouldTransformRoute
                        ?.Invoke(template, modelContext) ?? true;

                if (!shouldTransformRoute)
                {
                    continue;
                }

                Options.Route.Hooks.BeforeRouteTransform?.Invoke(template, modelContext);

                var newTemplate = RouteTransformer.TransformRouteTemplate(template, caseConverter);

                // Transform parameters in route
                if (Options.Route.RazorPages.TransformParameterNames)
                {
                    newTemplate = RouteTransformer.TransformRouteParameters(
                        newTemplate,
                        modelContext,
                        Options);
                }

                selector.AttributeRouteModel.Template = newTemplate;
                Options.Route.Hooks.AfterRouteTransform?.Invoke(newTemplate, template, modelContext);
            }
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
