using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.UI.TagHelpers
{
    /// <summary>
    /// Base Tag Helper that provides common functionality for all convention-based Tag Helpers.
    /// </summary>
    /// <param name="options">The convention options, injected via dependency injection.</param>
    public abstract class ConventionTagHelper(IOptions<AspNetConventionOptions> options) : TagHelper
    {
        /// <summary>
        /// Delegate that can be assigned to perform additional processing during the Tag Helper's Process method.
        /// </summary>
        /// <param name="context">The tag helper context.</param>
        /// <param name="output">The tag helper output.</param>
        /// <param name="propertyName">The original property name.</param>
        /// <param name="transformedName">The transformed property name.</param>
        public delegate void OnProcessCallback(
            TagHelperContext context,
            TagHelperOutput output,
            string propertyName,
            string transformedName);

        // Cache to store transformed property names for performance optimization
        internal readonly static ConcurrentDictionary<string, string> _propertyCache = new(StringComparer.OrdinalIgnoreCase);

        // Store the convention options
        internal readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        // Define the constant for the "asp-for" attribute name
        internal const string ForAttributeName = "asp-for";

        // Set a high order value to ensure that this Tag Helper runs after most 
        // other Tag Helpers, allowing it to modify the output as needed.
        public override int Order => 10000;

        /// <summary>
        /// The ModelExpression representing the model property that this Tag Helper is associated with. 
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public virtual required ModelExpression ForModel { get; set; } = default!;

        /// <summary>
        /// The ViewContext, which provides access to the current view's context, including the model metadata.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        /// <summary>
        /// An optional callback that can be assigned by derived Tag Helpers.
        /// </summary>
        public OnProcessCallback? OnProcess { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (output == null)
            {
                return;
            }

            string? propertyName;

            // Check if the transformed property name is already cached
            if (_propertyCache.TryGetValue(ForModel.Name, out var propertyCache))
            {
                propertyName = propertyCache;
            }
            else
            {
                // Get the original property name and transform it using the case converter
                var caseConverter = _options.Value.Route.GetCaseConverter();
                propertyName = caseConverter.Convert(GetPropertyName());

                // Cache the transformed property name
                _propertyCache[ForModel.Name] = propertyName;
            }

            // If an OnProcess callback is defined, invoke it to allow for custom 
            // processing of the tag output. This allows derived Tag Helpers to modify the output.
            if (OnProcess != null)
            {
                OnProcess.Invoke(context, output, ForModel.Name, propertyName);
                return;
            }

            // Set "name" attribute (default for input, textarea, select) to the 
            // transformed property name to ensure correct model binding.
            output.Attributes.RemoveAll("name");
            output.Attributes.Add("name", propertyName);
        }

        /// <summary>
        /// This method ensures that the correct property name is used for transformation, especially in 
        /// cases where the model has nested properties or custom binder names.
        /// </summary>
        /// <returns>The property name to be transformed.</returns>
        private string GetPropertyName()
        {
            // Split the model expression name to handle nested properties
            var parts = ForModel.Name.Split('.');

            // Get the property name from the metadata, considering any custom binder model names
            var propertyName = ForModel.Metadata.BinderModelName ?? ForModel.Metadata.PropertyName;

            if (parts.Length == 1 || string.IsNullOrEmpty(propertyName))
            {
                return propertyName ?? ForModel.Name;
            }

            // Get container metadata
            var containerName = parts[0];
            var modelMetadata = ViewContext.ViewData.ModelMetadata;
            var containerMetadata = modelMetadata?.Properties
                .FirstOrDefault(p => p.PropertyName == containerName);

            // Get the container prefix
            var prefix = containerMetadata?.BinderModelName ?? containerName;

            // Handle nested properties
            if (parts.Length > 2)
            {
                var currentMetadata = containerMetadata;
                var nestedParts = new HashSet<string> { prefix };

                // Iterate through the nested properties and build the 
                // transformed name, considering any custom binder model names
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    var nestedProperty = currentMetadata?.Properties
                        .FirstOrDefault(p => p.PropertyName == parts[i]);

                    var nestedName = nestedProperty?.BinderModelName ?? parts[i];

                    nestedParts.Add(nestedName);
                    currentMetadata = nestedProperty;
                }

                nestedParts.Add(propertyName);
                return string.Join('.', nestedParts);
            }

            return $"{prefix}.{propertyName}";
        }
    }
}
