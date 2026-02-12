using System;
using System.Collections.Generic;
using AspNetConventions.Routing.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Contains comprehensive information about the bindability of a property or parameter.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all the metadata and analysis needed to determine how a property
    /// or parameter should be bound. It's used throughout the routing and binding system
    /// to make consistent decisions about parameter transformation and binding behavior.
    /// </remarks>
    public class BindingContext
    {
        private HashSet<string>? _bindInclude;
        private HashSet<string>? _bindExclude;

        /// <summary>
        /// Gets or sets the name of the property or parameter.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the model being bound.
        /// </summary>
        public required Type ModelType { get; set; }

        /// <summary>
        /// Gets the effective model name used for binding.
        /// </summary>
        /// <value>The binder model name if set, otherwise the property/parameter name.</value>
        public string ModelName => BinderModelName ?? Name;

        /// <summary>
        /// Gets or sets the type that contains this property or parameter.
        /// </summary>
        /// <value>The declaring type for properties, or the containing type for parameters.</value>
        public Type? ContainerType { get; set; }

        /// <summary>
        /// Gets or sets the kind of metadata (Property or Parameter).
        /// </summary>
        public ModelMetadataKind MetadataKind { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property or parameter can be bound.
        /// </summary>
        /// <value>true if binding is allowed; otherwise, false.</value>
        /// <remarks>This is determined by analyzing binding attributes and framework rules.</remarks>
        public bool IsBindable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the binding source supports model name providers.
        /// </summary>
        /// <value>true if the binding source supports IModelNameProvider; otherwise, false.</value>
        public bool SupportsModelName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a complex bindable type.
        /// </summary>
        /// <value>true if the type is complex and bindable; otherwise, false.</value>
        /// <remarks>This is determined using the <see cref="ModelTypeClassifier"/>.</remarks>
        public bool IsComplexBindableType { get; set; }

        /// <summary>
        /// Gets or sets the explicit binder model name if specified.
        /// </summary>
        /// <value>The explicit model name from binding attributes, or null if not specified.</value>
        public string? BinderModelName { get; set; }

        /// <summary>
        /// Gets or sets the binding source for the property or parameter.
        /// </summary>
        /// <value>The binding source (Query, Form, Header, etc.), or null if not specified.</value>
        public BindingSource? BindingSource { get; set; }

        /// <summary>
        /// Gets the set of property names to include in binding.
        /// </summary>
        /// <value>Property names specified in Bind attributes, or empty if no restrictions.</value>
        public HashSet<string> BindInclude => _bindInclude ??= [];

        /// <summary>
        /// Gets the set of property names to exclude from binding.
        /// </summary>
        /// <value>Property names to exclude, typically empty unless explicitly specified.</value>
        public HashSet<string> BindExclude => _bindExclude ??= [];
    }
}
