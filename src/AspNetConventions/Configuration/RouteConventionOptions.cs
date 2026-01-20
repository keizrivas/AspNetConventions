using System;
using AspNetConventions.Common.Enums;
using AspNetConventions.Common.Hooks;
using AspNetConventions.Routing.Abstractions;
using AspNetConventions.Routing.Converters;

namespace AspNetConventions.Configuration
{
    /// <summary>
    /// Provides configuration options for route naming conventions.
    /// </summary>
    public sealed class RouteConventionOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether route transformations are enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public RazorPageOptions RazorPages { get; set; } = new();

        public ControllerOptions Controllers { get; set; } = new();

        public MinimalApiOptions MinimalApi { get; set; } = new();

        /// <summary>
        /// Gets or sets the casing style for routes.
        /// </summary>
        public CasingStyle CaseStyle { get; set; } = CasingStyle.KebabCase;

        /// <summary>
        /// Gets or sets a custom case converter.
        /// Required when <see cref="CaseStyle"/> is <see cref="CasingStyle.Custom"/>.
        /// </summary>
        public ICaseConverter? CustomCaseConverter { get; set; }

        /// <summary>
        /// Gets or sets whether to transform controller names.
        /// </summary>
        public bool TransformRouteTokens { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform Razor Page routes.
        /// </summary>
        public bool TransformPages { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform route parameter names.
        /// </summary>
        public bool TransformParameterNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform Minimal API parameter names.
        /// </summary>
        public bool TransformMinimalApiParameterNames { get; set; }

        /// <summary>
        /// Gets or sets whether to preserve explicitly named parameters.
        /// When true, parameters with explicit [FromRoute(Name = "...")] are not transformed.
        /// </summary>
        public bool PreserveExplicitBindingNames { get; set; }

        //public bool PreserveExplicitParameterNames { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed route template length for security purposes.
        /// Default is 2048 characters.
        /// </summary>
        public int MaxRouteTemplateLength { get; set; } = 2048;

        /// <summary>
        /// Gets or sets the collection of hooks used to customize route convention behavior.
        /// </summary>
        public RouteConventionHooks Hooks { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="RouteConventionOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            return new RouteConventionOptions
            {
                IsEnabled = IsEnabled,
                CaseStyle = CaseStyle,
                TransformPages = TransformPages,
                TransformRouteTokens = TransformRouteTokens,
                TransformParameterNames = TransformParameterNames,
                TransformMinimalApiParameterNames = TransformMinimalApiParameterNames,
                PreserveExplicitBindingNames = PreserveExplicitBindingNames,
                CustomCaseConverter = CustomCaseConverter,
                Hooks = Hooks,
            };
        }

        /// <summary>
        /// Gets the case converter for this configuration.
        /// </summary>
        internal ICaseConverter GetCaseConverter()
        {
            return CaseStyle switch
            {
                CasingStyle.KebabCase  => CaseConverterFactory.CreateKebabCase(),
                CasingStyle.SnakeCase  => CaseConverterFactory.CreateSnakeCase(),
                CasingStyle.CamelCase  => CaseConverterFactory.CreateCamelCase(),
                CasingStyle.PascalCase => CaseConverterFactory.CreatePascalCase(),
                CasingStyle.Custom => CustomCaseConverter ?? throw new InvalidOperationException("CustomConverter not set"),
                _ => CaseConverterFactory.CreateKebabCase()
            };
        }
    }
}
