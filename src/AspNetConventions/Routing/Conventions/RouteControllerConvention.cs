using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Configuration;
using AspNetConventions.Routing.Abstractions;
using AspNetConventions.Routing.ModelBinding;
using AspNetConventions.Routing.Models;
using AspNetConventions.Routing.Parsers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Applies route naming and parameter conventions to controllers and actions based on the configured ASP.NET
    /// conventions options.
    /// </summary>
    /// <remarks>This convention is intended for use with ASP.NET Core applications to standardize route
    /// templates and parameter names according to project-wide settings.</remarks>
    /// <param name="Options">The options used to configure conventions.</param>
    internal sealed class RouteControllerConvention(IOptions<AspNetConventionOptions> Options) : ConventionOptions(Options), IControllerModelConvention
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

            foreach (var action in controller.Actions)
            {
                foreach (var selector in action.Selectors)
                {
                    // Check if action selector has a route template
                    var template = selector.AttributeRouteModel?.Template;
                    if (selector.AttributeRouteModel == null || string.IsNullOrWhiteSpace(template))
                    {
                        continue;
                    }

                    var model = new RouteModelContext(selector, action);
                    var baseTemplate = RouteTemplateManager.GetFullRouteTemplate(model) ?? string.Empty;

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

                    var newTemplate = RouteTemplateManager.GetFullRouteTemplate(model) ?? string.Empty;
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
                    RouteTemplateManager.TransformRouteTemplate(template, Options.Route.GetCaseConverter());
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

                // Transform action route
                template = RouteTemplateManager.TransformRouteTemplate(template, Options.Route.GetCaseConverter());

                var model = new RouteModelContext(selector, action);
                if (Options.Route.TransformParameterNames)
                {
                    // Apply parameter name binding
                    ApplyParameterBinding(model);

                    // Transform parameters in route
                    ApplyConventionToRouteParameters(model, ref template);
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

        private string TransformParameterName(RouteModelContext modelContext, string name, bool isExplicitName)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "The \"name\" param can't be null or white space");
            }

            // Skip if preserving explicit names
            if (isExplicitName && Options.Route.PreserveExplicitBindingNames)
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
    }
}
