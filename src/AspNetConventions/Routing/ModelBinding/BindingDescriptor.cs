using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetConventions.Routing.ModelBinding
{
    /// <summary>
    /// Unified descriptor to extract binding context from both Metadata and Application Model pipelines.
    /// </summary>
    public static class BindingDescriptor
    {
        /// <summary>
        /// Extracts binding context information from a BindingMetadataProviderContext, which is used in 
        /// the model metadata pipeline for both MVC and Razor Pages.
        /// </summary>
        /// <param name="context">The binding metadata provider context to extract information from.</param>
        /// <returns>A <see cref="BindingContext"/> containing the extracted binding information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public static BindingContext GetBindingContext(BindingMetadataProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            return CreateContext(
                context.Key.Name ?? string.Empty,
                context.Key.MetadataKind,
                context.Key.ModelType,
                context.Key.ContainerType,
                context.Attributes,
                context.BindingMetadata?.BindingSource,
                context.BindingMetadata?.BinderModelName
            );
        }

        /// <summary>
        /// Extracts binding context information from a ParameterModelBase, which can represent both MVC and 
        /// Razor Page parameters and properties.
        /// </summary>
        /// <param name="modelBase">The parameter model to extract binding context from.</param>
        /// <returns>A <see cref="BindingContext"/> containing the extracted binding information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="modelBase"/> is null.</exception>
        public static BindingContext GetBindingContext(ParameterModelBase modelBase)
        {
            ArgumentNullException.ThrowIfNull(modelBase, nameof(modelBase));

            var (name, kind, container) = modelBase switch
            {
                // Razor Pages
                PagePropertyModel p => (p.PropertyName, ModelMetadataKind.Property, p.PropertyInfo.DeclaringType),
                PageParameterModel p => (p.ParameterName, ModelMetadataKind.Parameter, p.ParameterInfo.Member?.DeclaringType),

                // MVC Controllers
                PropertyModel p => (p.PropertyName, ModelMetadataKind.Property, p.PropertyInfo.DeclaringType),
                ParameterModel p => (p.ParameterName, ModelMetadataKind.Parameter, p.ParameterInfo.Member?.DeclaringType),

                // Future-proof fallback
                _ => (modelBase.Name, ModelMetadataKind.Parameter, null)
            };

            return CreateContext(
                name,
                kind,
                modelBase.ParameterType,
                container,
                modelBase.Attributes,
                modelBase.BindingInfo?.BindingSource,
                modelBase.BindingInfo?.BinderModelName
            );
        }

        /// <summary>
        /// Creates a BindingContext based on the provided parameters, applying conventions and determining bindability.
        /// </summary>
        /// <param name="name">The name of the model or parameter.</param>
        /// <param name="kind">The kind of metadata (parameter or property).</param>
        /// <param name="modelType">The type of the model being bound.</param>
        /// <param name="containerType">The type of the container (e.g., class) that holds the model, if applicable.</param>
        /// <param name="attributes">The attributes applied to the model or parameter.</param>
        /// <param name="source">The binding source, if specified.</param>
        /// <param name="existingBinderName">An existing binder model name, if specified.</param>
        /// <returns>A <see cref="BindingContext"/> with the appropriate properties set based on conventions and attributes.</returns>
        private static BindingContext CreateContext(
            string name,
            ModelMetadataKind kind,
            Type modelType,
            Type? containerType,
            IEnumerable<object> attributes,
            BindingSource? source,
            string? existingBinderName)
        {
            var attrList = attributes as IReadOnlyList<object> ?? [.. attributes];

            var context = new BindingContext
            {
                Name = name,
                MetadataKind = kind,
                ModelType = modelType,
                ContainerType = containerType,
                BindingSource = source,
                SupportsModelName = attrList.OfType<IModelNameProvider>().Any()
            };

            context.IsBindable = DetermineBindability(context, attrList);

            if (!context.IsBindable)
            {
                return context;
            }

            // Check if it's a complex bindable type
            context.IsComplexBindableType
                = ModelTypeClassifier.IsComplexBindableType(modelType);

            // Resolve the active model name
            context.BinderModelName = !string.IsNullOrWhiteSpace(existingBinderName)
                ? existingBinderName
                : attrList.OfType<IModelNameProvider>().FirstOrDefault()?.Name;

            // Handle Bind Include
            var bindAttr = attrList.OfType<BindAttribute>().FirstOrDefault();
            if (bindAttr?.Include != null)
            {
                context.BindInclude.UnionWith(bindAttr.Include);
            }

            return context;
        }

        /// <summary>
        /// Determines whether the model name should be set for the given binding context, based on bindability and explicit naming rules.
        /// </summary>
        /// <param name="bindingContext">The binding context to evaluate.</param>
        /// <param name="name">Outputs the model name to set if applicable.</param>
        /// <returns>True if the model name should be set; otherwise, false.</returns>
        public static bool ShouldSetBinderModelName(BindingContext bindingContext, out string name)
        {
            name = string.Empty;
            ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

            // Determine whether the context is bindable.
            if (!bindingContext.IsBindable)
            {
                return false;
            }

            // Do not set the model name for complex types without an explicit name unless
            // the binding source supports the IModelNameProvider interface.
            // See: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-8.0#complex-types

            var shouldSetModelName = !(bindingContext.SupportsModelName &&
                bindingContext.IsComplexBindableType &&
                string.IsNullOrEmpty(bindingContext.BinderModelName));

            if (shouldSetModelName)
            {
                name = bindingContext.ModelName;
            }

            return shouldSetModelName;
        }

        /// <summary>
        /// Determines whether a property should be included in model binding based on the BindInclude 
        /// settings in the parent context.
        /// </summary>
        /// <param name="propertyName">The name of the property to evaluate for inclusion.</param>
        /// <param name="parentContext">The parent binding context that may contain BindInclude settings.</param>
        /// <returns>True if the property should be included in model binding; otherwise, false.</returns>
        public static bool ShouldIncludeProperty(
            string propertyName,
            BindingContext parentContext)
        {
            ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
            ArgumentNullException.ThrowIfNull(parentContext, nameof(parentContext));

            if (parentContext.BindInclude.Count == 0)
            {
                return true;
            }

            return parentContext.BindInclude
                .Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines the bindability of a model or parameter based on its attributes and binding source.
        /// </summary>
        /// <param name="context">The binding context containing metadata about the model or parameter.</param>
        /// <param name="attributes">The attributes applied to the model or parameter.</param>
        /// <returns>True if the model or parameter is bindable; otherwise, false.</returns>
        private static bool DetermineBindability(BindingContext context, IReadOnlyList<object> attributes)
        {
            // Explicit exclusions
            foreach (var attr in attributes)
            {
                if (attr is BindNeverAttribute)
                {
                    return false;
                }

                if (attr is BindableAttribute { Bindable: false })
                {
                    return false;
                }

                if (attr is FromServicesAttribute)
                {
                    return false;
                }
            }

            return context.MetadataKind switch
            {
                // Parameters are bindable by default
                ModelMetadataKind.Parameter => true,

                // Razor Pages property rules
                ModelMetadataKind.Property =>
                    context.BindingSource != null ||
                    attributes.Any(a => a is BindPropertyAttribute),
                _ => false
            };
        }
    }
}
