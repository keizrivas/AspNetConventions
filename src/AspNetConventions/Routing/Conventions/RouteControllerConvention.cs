using System;
using System.Collections.Generic;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Routing.ModelBinding;
using AspNetConventions.Routing.Models;
using AspNetConventions.Routing.Parsers;
using AspNetConventions.Routing.Transformation;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Applies route naming and parameter conventions to controllers and actions based on the configured ASP.NET
    /// conventions options.
    /// </summary>
    /// <param name="options">The options used to configure conventions.</param>
    /// <remarks>
    /// This convention is intended for use with ASP.NET Core applications to standardize route
    /// templates and parameter names according to project-wide settings.
    /// </remarks>
    internal sealed class RouteControllerConvention(IOptions<AspNetConventionOptions> options) : ConventionOptions(options), IControllerModelConvention
    {
        private static readonly HashSet<RouteParameterContext> _explicitNameCache = [];
        private static readonly Dictionary<RouteParameterContext, bool> _parameterTransformCache = [];

        public void Apply(ControllerModel controller)
        {
            // Setup options
            CreateOptionSnapshot();

            // Check if routing conventions are enabled
            if (!Options.Route.IsEnabled)
            {
                return;
            }

            // Skip excluded controllers
            if (ContainsOrdinalIgnoreCase(Options.Route.Controllers.ExcludeControllers, controller.ControllerName))
            {
                return;
            }

            // Skip excluded areas
            if (Options.Route.Controllers.ExcludeAreas.Count > 0
                && controller.RouteValues.TryGetValue("area", out var area)
                && area != null
                && ContainsOrdinalIgnoreCase(Options.Route.Controllers.ExcludeAreas, area))
            {
                return;
            }

            foreach (var action in controller.Actions)
            {
                // Set up route parameter transformer for this action
                action.RouteParameterTransformer = new RouteTokenTransformer(options);

                foreach (var selector in action.Selectors)
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

                    var model = RouteModelContext.FromMvcAction(selector, action);
                    var baseTemplate = RouteTransformer.GetRouteTemplate(model) ?? string.Empty;

                    // Determine if route should be transformed
                    var shouldTransformRoute = Options.Route.Hooks.ShouldTransformRoute
                            ?.Invoke(baseTemplate, model) ?? true;

                    if (!shouldTransformRoute)
                    {
                        continue;
                    }

                    Options.Route.Hooks.BeforeRouteTransform?.Invoke(baseTemplate, model);

                    // Apply conventions
                    ApplyConventionToController(controller);
                    ApplyConventionToAction(action);

                    var newTemplate = RouteTransformer.GetRouteTemplate(model) ?? string.Empty;
                    Options.Route.Hooks.AfterRouteTransform?.Invoke(newTemplate, baseTemplate, model);
                }
            }
        }

        /// <summary>
        /// Applies conventions to the specified controller model.
        /// </summary>
        /// <param name="controller">The controller model to which the conventions are applied.</param>
        private void ApplyConventionToController(ControllerModel controller)
        {
            // Transform controller route templates
            foreach (var selector in controller.Selectors)
            {
                var template = selector.AttributeRouteModel?.Template;
                if (selector.AttributeRouteModel == null || string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                selector.AttributeRouteModel.Template =
                    RouteTransformer.TransformRouteTemplate(template, Options.Route.GetCaseConverter());
            }
        }

        /// <summary>
        /// Applies route and parameter transformation conventions to the specified action model.
        /// </summary>
        /// <param name="action">The action model to which the conventions are applied.</param>
        private void ApplyConventionToAction(ActionModel action)
        {
            foreach (var selector in action.Selectors)
            {
                var template = selector.AttributeRouteModel?.Template;
                if (selector.AttributeRouteModel == null || string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                // Replace [action] token before TransformRouteTemplate runs so that prefix/suffix
                // stripping has access to the original action name and excluded-action context.
                if (template.Contains("[action]", StringComparison.OrdinalIgnoreCase))
                {
                    var actionName = StripActionPrefixesAndSuffixes(action.ActionName);
                    var converted  = Options.Route.GetCaseConverter().Convert(actionName);
                    template = template.Replace("[action]", converted, StringComparison.OrdinalIgnoreCase);
                }

                // Transform action route
                template = RouteTransformer.TransformRouteTemplate(template, Options.Route.GetCaseConverter());

                var modelContext = RouteModelContext.FromMvcAction(selector, action);
                if (Options.Route.Controllers.TransformParameterNames)
                {
                    // Apply parameter name binding
                    ApplyParameterBinding(modelContext);

                    // Transform parameters in route
                    ApplyConventionToRouteParameters(modelContext, ref template);
                }

                selector.AttributeRouteModel.Template = template;
            }
        }

        /// <summary>
        /// Applies parameter name transformation and binding information to the parameters of the
        /// specified action model.
        /// </summary>
        /// <param name="modelContext">Route model data</param>
        private void ApplyParameterBinding(RouteModelContext modelContext)
        {
            foreach (var param in modelContext.Action!.Parameters)
            {
                var bindingContext = BindingDescriptor.GetBindingContext(param);

                // Skip if should not set binder model name
                if (!BindingDescriptor.ShouldSetBinderModelName(bindingContext, out var name))
                {
                    continue;
                }

                // Set explicit name cache for path parameters
                if (bindingContext.BinderModelName != null
                    && bindingContext.BindingSource != null
                    && bindingContext.BindingSource.CanAcceptDataFrom(BindingSource.Path))
                {
                    var parameterContext = new RouteParameterContext(modelContext, name);
                    _explicitNameCache.Add(parameterContext);
                }

                // Transform parameter name
                name = TransformParameterName(
                    modelContext,
                    name,
                    !string.IsNullOrEmpty(bindingContext.BinderModelName));

                param.BindingInfo ??= new BindingInfo();
                param.BindingInfo.BinderModelName = name;
            }
        }

        /// <summary>
        /// Applies parameter name transformation to the route parameters in the specified route template.
        /// </summary>
        /// <param name="modelContext">Route model data</param>
        /// <param name="template">The route model template</param>
        private void ApplyConventionToRouteParameters(RouteModelContext modelContext, ref string template)
        {
            // Transform parameter names in the route template
            template = RouteParameterPatterns.ForEachParam(template, (name, constraint) =>
            {
                // Create the route key
                var paramName = RouteParameterPatterns.CleanParameterName(name);
                var parameterContext = new RouteParameterContext(modelContext, paramName);

                name = TransformParameterName(
                    modelContext,
                    name,
                    _explicitNameCache.Contains(parameterContext));

                return "{" + name + constraint + "}";
            });
        }

        /// <summary>
        /// Transforms a route parameter name based on the configured conventions and binding information.
        /// </summary>
        /// <param name="modelContext">The route model context for the parameter.</param>
        /// <param name="name">The original name of the route parameter.</param>
        /// <param name="isExplicitName">Indicates whether the parameter has an explicit name defined via binding attributes.</param>
        /// <returns>The transformed parameter name according to the configured conventions.</returns>
        private string TransformParameterName(RouteModelContext modelContext, string name, bool isExplicitName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "The \"name\" param can't be null or white space");
            }

            // Skip if preserving explicit names
            if (isExplicitName && Options.Route.Controllers.PreserveExplicitBindingNames)
            {
                return name;
            }

            // Determine if parameter should be transformed
            var paramName = RouteParameterPatterns.CleanParameterName(name);
            var parameterContext = new RouteParameterContext(modelContext, paramName);

            if (!_parameterTransformCache.TryGetValue(parameterContext, out var shouldTransformParameter))
            {
                shouldTransformParameter = Options.Route.Hooks.ShouldTransformParameter
                    ?.Invoke(parameterContext) ?? true;

                _parameterTransformCache[parameterContext] = shouldTransformParameter;
            }

            return shouldTransformParameter
                ? Options.Route.GetCaseConverter().Convert(name)
                : name;
        }

        private string StripActionPrefixesAndSuffixes(string actionName)
        {
            foreach (var prefix in Options.Route.Controllers.RemoveActionPrefixes)
            {
                if (actionName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    actionName = actionName[prefix.Length..];
                    break;
                }
            }

            foreach (var suffix in Options.Route.Controllers.RemoveActionSuffixes)
            {
                if (actionName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    actionName = actionName[..^suffix.Length];
                    break;
                }
            }

            return actionName;
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
