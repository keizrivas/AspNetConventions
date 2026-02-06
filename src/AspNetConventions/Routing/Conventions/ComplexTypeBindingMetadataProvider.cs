using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Routing.ModelBinding;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Apply conventions to user-defined complex types properties
    /// </summary>
    /// <param name="options">The convention options for routing and binding.</param>
    internal sealed class ComplexTypeBindingMetadataProvider(IOptions<AspNetConventionOptions> options) : ConventionOptions(options), IBindingMetadataProvider
    {
        // Cache for complex type parameters discovered at startup
        private static readonly ConcurrentDictionary<Type, ComplexTypeInfo> _complexTypeCache = new();

        // Cache for already-processed properties to avoid redundant transformations
        private static readonly ConcurrentDictionary<string, bool> _processedProperties = new();

        /// <summary>
        /// Creates binding metadata for complex type properties based on the configured conventions.
        /// </summary>
        /// <param name="context">The context for which to create binding metadata.</param>
        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            CreateOptionSnapshot();

            // Early exit if transformation is disabled
            if (!Options.Route.IsEnabled)
            {
                return;
            }

            var key = context.Key;

            // Only process Properties and Parameters
            if (key.MetadataKind != ModelMetadataKind.Property &&
                key.MetadataKind != ModelMetadataKind.Parameter)
            {
                return;
            }

            //IMPORTANT:
            //Razor Pages use both the handler parameter and the complex-type property metadata
            //to build 'binding keys'.The IBindingMetadataProvider is called multiple times for
            //the same logical property.Parameter metadata is used for input binding, while property
            //metadata is used for rendering / validation(output).
            //See: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0

            // Handle Parameters
            if (key.MetadataKind == ModelMetadataKind.Parameter)
            {
                HandleParameterMetadata(context);
                return;
            }

            // Handle Properties
            HandlePropertyMetadata(context);
        }

        /// <summary>
        /// Handles Parameter metadata
        /// </summary>
        /// <param name="context">The context containing the parameter metadata to process.</param>
        private void HandleParameterMetadata(BindingMetadataProviderContext context)
        {
            var key = context.Key;
            var parameterType = key.ModelType;

            // Skip if not safe to transform
            if (!IsSafeUserDefinedType(parameterType, context))
            {
                return;
            }

            // Check if it's a complex bindable type
            var isComplexType = ModelTypeClassifier.IsComplexBindableType(parameterType);

            if (isComplexType)
            {
                // This is a complex type parameter - cache it for later property processing
                // e.g., OnGet([FromQuery] QueryInput query)
                CacheComplexType(parameterType);
            }
            else
            {
                // This is a simple type parameter OR a property of a complex type
                // Check if this belongs to a cached complex type
                if (IsPropertyOfComplexType(context, out var rootComplexType))
                {
                    // This is a property of a complex type being processed as Parameter
                    // Transform it!
                    TransformBindingName(context, rootComplexType);
                }
            }
        }

        /// <summary>
        /// Handles Property metadata
        /// </summary>
        /// <param name="context">The context containing the property metadata to process.</param>
        private void HandlePropertyMetadata(BindingMetadataProviderContext context)
        {
            var key = context.Key;
            var containerType = key.ContainerType;

            // Skip if no container
            if (containerType == null)
            {
                return;
            }

            // Skip framework types
            if (!IsSafeUserDefinedType(containerType, context))
            {
                return;
            }

            // Check if this property belongs to a cached complex type
            if (IsPropertyOfComplexType(context, out var rootComplexType))
            {
                // This property belongs to a user-defined complex type
                // Transform it!
                TransformBindingName(context, rootComplexType);
            }
            else
            {
                // This might be a PageModel property or other framework property
                // Check if the container itself is a complex type we should process
                if (ModelTypeClassifier.IsComplexBindableType(containerType) &&
                    IsSafeUserDefinedType(containerType, context))
                {
                    // This is a property of a user-defined complex type
                    // that hasn't been cached yet (Razor Pages scenario)
                    CacheComplexType(containerType);
                    TransformBindingName(context, containerType);
                }
            }
        }

        /// <summary>
        /// Performs the actual transformation of the binding name
        /// </summary>
        /// <param name="context">The context containing the metadata to transform.</param>
        /// <param name="rootComplexType">The root complex type that this property belongs to, used for caching.</param>
        private void TransformBindingName(
            BindingMetadataProviderContext context,
            Type rootComplexType)
        {
            var key = context.Key;

            // Check if already processed
            var prefix = rootComplexType.Name;
            var cacheKey = $"{rootComplexType.FullName}.{key.Name}.{key.MetadataKind}";
            if (_processedProperties.ContainsKey(cacheKey))
            {
                return;
            }

            // Check for explicit binding name preservation
            if (context.BindingMetadata.BinderModelName != null &&
                (Options.Route.Controllers.PreserveExplicitBindingNames || Options.Route.RazorPages.PreserveExplicitBindingNames))
            {
                return;
            }

            var sourceName = context.BindingMetadata.BinderModelName ?? key.Name;
            if (string.IsNullOrEmpty(sourceName))
            {
                return;
            }

            var transformed = Options.Route.GetCaseConverter().Convert(sourceName);

            context.BindingMetadata.BinderModelName = transformed;

            // Mark as processed
            _processedProperties.TryAdd(cacheKey, true);
        }

        /// <summary>
        /// Caches a complex type for tracking its properties
        /// </summary>
        /// <param name="complexType">The complex type to cache.</param>
        private static void CacheComplexType(Type complexType)
        {
            if (_complexTypeCache.ContainsKey(complexType))
            {
                return;
            }

            var properties = complexType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToHashSet();

            var info = new ComplexTypeInfo
            {
                Type = complexType,
                PropertyNames = properties
            };

            _complexTypeCache.TryAdd(complexType, info);
        }

        /// <summary>
        /// Checks if the current context represents a property of a cached complex type
        /// </summary>
        /// <param name="context">The context to evaluate.</param>
        /// <param name="rootComplexType">Outputs the root complex type if a match is found.</param>
        /// <returns>True if the context is a property of a cached complex type; otherwise, false.</returns>
        private static bool IsPropertyOfComplexType(
            BindingMetadataProviderContext context,
            out Type? rootComplexType)
        {
            rootComplexType = null;
            var key = context.Key;

            // For Parameters: Check if this parameter name matches a property in any cached type
            if (key.MetadataKind == ModelMetadataKind.Parameter)
            {
                // Razor Pages scenario: Properties are processed as Parameters first
                // Check if this parameter name exists in any cached complex type
                foreach (var (type, info) in _complexTypeCache)
                {
                    if (info.PropertyNames.Contains(key.Name))
                    {
                        rootComplexType = type;
                        return true;
                    }
                }

                // Not found in cache
                return false;
            }

            // For Properties: Check the container type
            if (key.MetadataKind == ModelMetadataKind.Property)
            {
                var containerType = key.ContainerType;
                if (containerType == null)
                {
                    return false;
                }

                // Direct match - the container is a cached complex type
                if (_complexTypeCache.ContainsKey(containerType))
                {
                    rootComplexType = containerType;
                    return true;
                }

                // Check if container is a nested type within a cached complex type
                foreach (var (cachedType, _) in _complexTypeCache)
                {
                    if (IsNestedWithin(containerType, cachedType))
                    {
                        rootComplexType = cachedType;
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Checks if childType is nested within parentType through property relationships
        /// </summary>
        /// <param name="childType">The type to check for being nested.</param>
        /// <param name="parentType">The type to check against for nesting.</param>
        private static bool IsNestedWithin(Type childType, Type parentType)
        {
            if (parentType == childType)
            {
                return true;
            }

            var properties = parentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;

                // Handle nullable types
                var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

                // Direct match
                if (underlyingType == childType)
                {
                    return true;
                }

                // Handle generic collections
                if (underlyingType.IsGenericType)
                {
                    var genericArgs = underlyingType.GetGenericArguments();
                    if (genericArgs.Any(arg => arg == childType))
                    {
                        return true;
                    }

                    // Recursively check nested generic types
                    foreach (var arg in genericArgs)
                    {
                        if (ModelTypeClassifier.IsComplexBindableType(arg) &&
                            IsNestedWithin(childType, arg))
                        {
                            return true;
                        }
                    }
                }

                // Recursive check for nested complex types
                if (ModelTypeClassifier.IsComplexBindableType(underlyingType) &&
                    IsNestedWithin(childType, underlyingType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a type is safe to transform (user-defined, not framework)
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <param name="context">The context for additional metadata checks.</param>
        private static bool IsSafeUserDefinedType(
            Type type,
            BindingMetadataProviderContext context)
        {
            // Check namespace
            var ns = type.Namespace;

            // Skip System and Microsoft framework types
            if (string.IsNullOrEmpty(ns) ||
                ns.StartsWith("System.", StringComparison.Ordinal) ||
                ns.StartsWith("Microsoft.", StringComparison.Ordinal))
            {
                return false;
            }

            // Skip ASP.NET Core base types
            if (type == typeof(PageModel) ||
                type.IsSubclassOf(typeof(PageModel)) ||
                type == typeof(Controller) ||
                type == typeof(ControllerBase) ||
                type.IsSubclassOf(typeof(Controller)) ||
                type.IsSubclassOf(typeof(ControllerBase)))
            {
                return false;
            }

            // Check binding source (only for parameters)
            if (context.Key.MetadataKind == ModelMetadataKind.Parameter)
            {
                var source = context.BindingMetadata.BindingSource;

                // Only transform query, form, and header bindings (support prefixs)
                if (source != null &&
                    source != BindingSource.Query &&
                    source != BindingSource.Form &&
                    source != BindingSource.Header &&
                    !source.CanAcceptDataFrom(BindingSource.Query))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
