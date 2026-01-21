using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Information about bindability of a property or parameter
    /// </summary>
    public class BindingContext
    {
        public required string Name { get; set; }
        public required Type ModelType { get; set; }
        public string ModelName => BinderModelName ?? Name;
        public Type? ContainerType { get; set; }
        public ModelMetadataKind MetadataKind { get; set; }
        public bool IsBindable { get; set; }
        public bool SupportsModelName { get; set; }
        public bool IsComplexBindableType { get; set; }
        public string? BinderModelName { get; set; }
        public BindingSource? BindingSource { get; set; }
        public HashSet<string> BindInclude { get; } = [];
        public HashSet<string> BindExclude { get; } = [];
    }
}
