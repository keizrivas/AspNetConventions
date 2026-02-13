using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Extensions;
using AspNetConventions.Routing.ModelBinding;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Apply conventions to user-defined complex types properties
    /// </summary>
    /// <param name="options">The convention options for routing and binding.</param>
    /// <param name="logger">The logger instance for logging convention processing details.</param>
    internal sealed class ComplexTypeBindingMetadataProvider(
        IOptions<AspNetConventionOptions> options,
        ILogger<ComplexTypeBindingMetadataProvider> logger) : ConventionOptions(options), IBindingMetadataProvider
    {
        // Maximum depth for nested type checking to prevent infinite recursion in complex object graphs
        private const int MaxNestingDepth = 10;

        // Use provided logger or fallback to NullLogger if null
        private readonly ILogger _logger = logger ?? NullLogger<ComplexTypeBindingMetadataProvider>.Instance;

        // Cache for already-processed properties to avoid redundant transformations
        private static readonly ConcurrentDictionary<string, bool> _processedProperties = new();

        // Cache for type properties to optimize nested type checks
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        // Cache for complex type parameters discovered at startup
        private static readonly ConcurrentDictionary<Type, ComplexTypeInfo> _complexTypeCache = new();

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

            if (!ShouldTransformBindingMetadata(context))
            {
                return;
            }

            // Extract binding context using BindingDescriptor
            var bindingContext = BindingDescriptor.GetBindingContext(context);

            //IMPORTANT:
            //Razor Pages use both the handler parameter and the complex-type property metadata
            //to build 'binding keys'.The IBindingMetadataProvider is called multiple times for
            //the same logical property. Parameter metadata is used for input binding, while property
            //metadata is used for rendering / validation(output).
            //See: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

            // Handle Parameters
            if (bindingContext.MetadataKind == ModelMetadataKind.Parameter)
            {
                HandleParameterMetadata(context, bindingContext);
                return;
            }

            // Handle Properties
            HandlePropertyMetadata(context, bindingContext);
        }

        /// <summary>
        /// Handles Parameter metadata
        /// </summary>
        /// <param name="context">The context containing the parameter metadata to process.</param>
        /// <param name="bindingContext">The binding context extracted from the metadata provider context.</param>
        private void HandleParameterMetadata(
            BindingMetadataProviderContext context,
            BindingContext bindingContext)
        {
            // Check if it's a complex bindable type
            if (bindingContext.IsComplexBindableType)
            {
                // This is a complex type parameter
                // cache it for later property processing
                CacheComplexType(bindingContext);
            }
            else
            {
                // This is a simple type parameter or a property of a complex type
                // Check if this belongs to a cached complex type
                if (IsPropertyOfComplexType(bindingContext, out var rootComplexType))
                {
                    // This is a property of a complex type being processed as Parameter
                    TransformBindingName(context, bindingContext, rootComplexType!);
                }
                else if (_logger.IsEnabled(LogLevel.Debug))
                {
                    // Simple standalone parameter, handled elsewhere
                    _logger.LogBindingMetadataDebug(
                        "PARAM-SIMPLE",
                        "Skipped",
                        $"{bindingContext.Name}",
                        "Simple standalone parameter.");
                }
            }
        }

        /// <summary>
        /// Handles Property metadata
        /// </summary>
        /// <param name="context">The context containing the property metadata to process.</param>
        /// <param name="bindingContext">The binding context extracted from the metadata provider context.</param>
        private void HandlePropertyMetadata(
            BindingMetadataProviderContext context,
            BindingContext bindingContext)
        {
            var containerType = bindingContext.ContainerType;

            // Skip if no container
            if (containerType == null)
            {
                return;
            }

            // Check if this property belongs to a cached complex type
            if (IsPropertyOfComplexType(bindingContext, out var rootComplexType))
            {
                // This property belongs to a user-defined complex type
                TransformBindingName(context, bindingContext, rootComplexType!);
            }
            else
            {
                // This might be a PageModel property or other framework property
                // Check if the container itself is a complex type we should process
                if (bindingContext.IsComplexBindableType &&
                    BindingDescriptor.ShouldSetBinderModelName(bindingContext, out _))
                {
                    // This is a property of a user-defined complex type
                    // that hasn't been cached yet (Razor Pages scenario)
                    CacheComplexType(bindingContext);
                    TransformBindingName(context, bindingContext, containerType);
                }
                else if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogBindingMetadataDebug(
                        "PROP",
                        "Skipped",
                        $"{containerType.Name}.{bindingContext.Name}",
                        "Not a complex or bindable type.");
                }
            }
        }

        /// <summary>
        /// Performs the actual transformation of the binding name
        /// </summary>
        /// <param name="context">The context containing the metadata to transform.</param>
        /// <param name="bindingContext">The binding context extracted from the metadata provider context.</param>
        /// <param name="rootComplexType">The root complex type that this property belongs to, used for caching.</param>
        private void TransformBindingName(
            BindingMetadataProviderContext context,
            BindingContext bindingContext,
            Type rootComplexType)
        {
            var debugKindName = string.Empty;
            var debugPrefixName = string.Empty;
            var debugIsEnabled = _logger.IsEnabled(LogLevel.Debug);
            var container = bindingContext.ContainerType;

            if (debugIsEnabled)
            {
                debugPrefixName = container != null && container.Name != rootComplexType.Name
                    ? $"{rootComplexType.Name}*.{container.Name}"
                    : rootComplexType.Name;

                debugKindName = bindingContext.MetadataKind == ModelMetadataKind.Parameter 
                    ? "PARAM" 
                    : "PROP";
            }

            // Build a cache key based on the root complex type, container, property name, and metadata kind
            var prefix = container != null && container.FullName != rootComplexType.FullName
                    ? $"{rootComplexType.FullName}.{container.Name}"
                    : rootComplexType.FullName;

            var cacheKey = $"{prefix}.{bindingContext.Name}.{bindingContext.MetadataKind}";

            // Check if already processed
            if (_processedProperties.ContainsKey(cacheKey))
            {
                if (debugIsEnabled)
                {
                    _logger.LogBindingMetadataDebug(
                        debugKindName,
                        "Skipped",
                        $"{debugPrefixName}.{bindingContext.Name}",
                        "Already processed.");
                }

                return;
            }

            // Check for explicit binding name preservation
            if (bindingContext.BinderModelName != null &&
                (Options.Route.Controllers.PreserveExplicitBindingNames || Options.Route.RazorPages.PreserveExplicitBindingNames))
            {
                if (debugIsEnabled)
                {
                    _logger.LogBindingMetadataDebug(
                        debugKindName,
                        "Skipped",
                        $"{debugPrefixName}.{bindingContext.Name} => {context.BindingMetadata.BinderModelName}",
                        "Explicit name preserved.");
                }

                return;
            }

            var sourceName = bindingContext.ModelName;
            if (string.IsNullOrEmpty(sourceName))
            {
                return;
            }

            var transformed = Options.Route.GetCaseConverter().Convert(sourceName);
            context.BindingMetadata.BinderModelName = transformed;

            if (debugIsEnabled)
            {
                _logger.LogBindingMetadataDebug(
                    debugKindName,
                    "Transform",
                    $"{debugPrefixName}.{bindingContext.Name} => {transformed}",
                    "Apply binder model name.");
            }

            // Mark as processed
            _processedProperties.TryAdd(cacheKey, true);
        }

        /// <summary>
        /// Caches a complex type for tracking its properties
        /// </summary>
        /// <param name="bindingContext">The binding context containing the complex type to cache.</param>
        private void CacheComplexType(BindingContext bindingContext)
        {
            var complexType = bindingContext.ModelType;
            if (_complexTypeCache.ContainsKey(complexType))
            {
                return;
            }

            // Get public instance properties of the complex type
            var properties = _propertyCache.GetOrAdd(
                complexType,
                t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            var propertyNames = new HashSet<string>(properties.Length, StringComparer.Ordinal);
            for (int i = 0; i < properties.Length; i++)
            {
                propertyNames.Add(properties[i].Name);
            }

            // Determine if this is a Razor Page model container
            var containerType = bindingContext.ContainerType;
            var razorPageContainer = typeof(PageModel).IsAssignableFrom(containerType) ||
                (containerType != null && typeof(PageModel) == containerType);

            var info = new ComplexTypeInfo
            {
                Type = complexType,
                PropertyNames = propertyNames,
                RazorPageContainer = razorPageContainer
            };

            // Cache the complex type information for later reference during property processing
            _complexTypeCache.TryAdd(complexType, info);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var type = bindingContext.IsComplexBindableType ? "COMPLEX" : "SIMPLE";
                var kind = bindingContext.MetadataKind == ModelMetadataKind.Parameter ? "PARAM" : "PROP";
                _logger.LogBindingMetadataDebug(
                    $"{kind}-{type}",
                    "Cached",
                    $"{bindingContext.Name} ({complexType.Name}) | ({propertyNames.Count}) props",
                    "Cache complex type info.");
            }
        }

        /// <summary>
        /// Checks if the current context represents a property of a cached complex type
        /// </summary>
        /// <param name="bindingContext">The binding context to evaluate.</param>
        /// <param name="rootComplexType">Outputs the root complex type if a match is found.</param>
        /// <returns>True if the context is a property of a cached complex type; otherwise, false.</returns>
        private static bool IsPropertyOfComplexType(
            BindingContext bindingContext,
            out Type? rootComplexType)
        {
            rootComplexType = null;

            // For Parameters: Check if this parameter name matches a property in any cached type
            if (bindingContext.MetadataKind == ModelMetadataKind.Parameter)
            {
                // Razor Pages scenario: Properties are processed as Parameters first
                // Check if this parameter name exists in any cached complex type
                foreach (var (type, info) in _complexTypeCache)
                {
                    if (info.PropertyNames.Contains(bindingContext.Name))
                    {
                        rootComplexType = type;
                        return true;
                    }
                }

                // Not found in cache
                return false;
            }

            // For Properties: Check the container type
            if (bindingContext.MetadataKind == ModelMetadataKind.Property)
            {
                var containerType = bindingContext.ContainerType;
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
        /// <param name="depth">The current depth of recursion to prevent infinite loops.</param>
        /// <returns>True if childType is nested within parentType; otherwise, false.</returns>
        private static bool IsNestedWithin(Type childType, Type parentType, int depth = 0)
        {
            if (depth > MaxNestingDepth)
            {
                return false;
            }

            if (parentType == childType)
            {
                return true;
            }

            depth++;
            var properties = _propertyCache.GetOrAdd(
                parentType,
                t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            // If no properties, can't be nested
            if (properties.Length == 0)
            {
                return false;
            }

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
                        if (ModelTypeClassifier.IsComplexType(arg) &&
                            IsNestedWithin(childType, arg, depth))
                        {
                            return true;
                        }
                    }
                }

                // Recursive check for nested complex types
                if (ModelTypeClassifier.IsComplexType(underlyingType) &&
                    IsNestedWithin(childType, underlyingType, depth))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the binding metadata should be transformed based on the context and conventions.
        /// </summary>
        /// <param name="context">The context containing the metadata to evaluate.</param>
        /// <returns>True if the metadata should be transformed; otherwise, false.</returns>
        private static bool ShouldTransformBindingMetadata(BindingMetadataProviderContext context)
        {
            // Only process Properties and Parameters
            if (context.Key.MetadataKind != ModelMetadataKind.Property &&
                context.Key.MetadataKind != ModelMetadataKind.Parameter)
            {
                return false;
            }

            // Determine the type to check
            Type? containerType = context.Key.ContainerType;

            // Skip framework base class containers (inherited properties)
            if (containerType != null && context.Key.MetadataKind == ModelMetadataKind.Property)
            {
                if (containerType == typeof(PageModel) ||
                    containerType == typeof(Controller) ||
                    containerType == typeof(ControllerBase) ||
                    containerType.IsSubclassOf(typeof(Controller)) ||
                    containerType.IsSubclassOf(typeof(ControllerBase)))
                {
                    return false;
                }
            }

            // Skip body parameters
            if(context.BindingMetadata.BindingSource == BindingSource.Body)
            {
                return false;
            }

            // [FromBody] is inferred for complex type parameters.
            // If a parameter type is a complex type and no binding source attribute [FromBody] is present,
            // model binding assumes it came from the body.
            // See: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?#frombody-attribute

            // Skip inferred [FromBody] parameters
            if (context.Key.MetadataKind == ModelMetadataKind.Parameter &&
                context.BindingMetadata.BindingSource == null &&
                ModelTypeClassifier.IsComplexType(context.Key.ModelType))
            {
                return false;
            }

            return true;
        }
    }
}
