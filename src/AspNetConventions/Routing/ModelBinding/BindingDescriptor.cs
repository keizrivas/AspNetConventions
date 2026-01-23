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
