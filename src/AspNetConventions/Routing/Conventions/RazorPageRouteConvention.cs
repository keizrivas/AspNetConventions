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

            var caseConverter = Options.Route.GetCaseConverter();
            foreach (var selector in pageModel.Selectors)
            {
                // Check if action selector has a route template
                var template = selector.AttributeRouteModel?.Template;
                if (selector.AttributeRouteModel == null || string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                var modelContext = new RouteModelContext(selector, pageModel);

                // Determine if route should be transformed
                var shouldTransformRoute = Options.Route.Hooks.ShouldTransformRoute
                        ?.Invoke(template, modelContext) ?? true;

                if (!shouldTransformRoute)
                {
                    continue;
                }

                Options.Route.Hooks.BeforeRouteTransform?.Invoke(template, modelContext);

                var newTemplate = RouteTemplateManager.TransformRouteTemplate(template, caseConverter);

                // Transform parameters in route
                if (Options.Route.RazorPages.TransformParameterNames)
                {
                    newTemplate = RouteTemplateManager.TransformRouteParameters(
                        newTemplate,
                        modelContext,
                        Options);
                }

                selector.AttributeRouteModel.Template = newTemplate;
                Options.Route.Hooks.AfterRouteTransform?.Invoke(newTemplate, template, modelContext);
            }
        }
    }
}
