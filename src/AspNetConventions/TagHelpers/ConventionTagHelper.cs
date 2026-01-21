using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace AspNetConventions.TagHelpers
{
    public abstract class ConventionTagHelper(IOptions<AspNetConventionOptions> options) : TagHelper
    {
        public delegate void OnProcessCallback(
            TagHelperContext context,
            TagHelperOutput output,
            string propertyName,
            string transformedName);

        internal readonly static ConcurrentDictionary<string, string> _propertyCache = new(StringComparer.OrdinalIgnoreCase);

        internal readonly IOptions<AspNetConventionOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

        internal const string ForAttributeName = "asp-for";

        private const string DataValidationForAttributeName = "data-valmsg-for";

        public override int Order => 10000;

        [HtmlAttributeName(ForAttributeName)]
        public required ModelExpression For { get; set; } = default!;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public OnProcessCallback? OnProcess { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (output == null)
            {
                return;
            }

            string? propertyName;
            if (_propertyCache.TryGetValue(For.Name, out var propertyCache))
            {
                propertyName = propertyCache;
            }
            else
            {
                var caseConverter = _options.Value.Route.GetCaseConverter();
                propertyName = caseConverter.Convert(GetPropertyName());
                _propertyCache[For.Name] = propertyName;
            }

            if (OnProcess != null)
            {
                OnProcess.Invoke(context, output, For.Name, propertyName);
                return;
            }

            // Set "name" attribute
            output.Attributes.RemoveAll("name");
            output.Attributes.Add("name", propertyName);

            // Set "id" attribute
            //output.Attributes.RemoveAll("id");
            //output.Attributes.Add("id", name);

            // Update validation attributes if present
            UpdateValidationAttributes(ref output, propertyName);
        }

        private string GetPropertyName()
        {
            var parts = For.Name.Split('.');
            var propertyName = For.Metadata.BinderModelName ?? For.Metadata.PropertyName;

            if (parts.Length == 1 || string.IsNullOrEmpty(propertyName))
            {
                return propertyName ?? For.Name;
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

        private static void UpdateValidationAttributes(ref TagHelperOutput output, string propertyName)
        {
            if (output.Attributes.TryGetAttribute(DataValidationForAttributeName, out var valmsgAttr))
            {
                output.Attributes.Remove(valmsgAttr);
                output.Attributes.Add(DataValidationForAttributeName, propertyName);
            }
        }
    }
}
