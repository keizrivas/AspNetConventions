using System;
using AspNetConventions.Configuration.Options.Route;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Converters;
using AspNetConventions.Core.Enums;
using AspNetConventions.Core.Hooks;

namespace AspNetConventions.Configuration.Options
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

        public RazorPagesRouteOptions RazorPages { get; set; } = new();

        public ControllerRouteOptions Controllers { get; set; } = new();

        public MinimalApiRouteOptions MinimalApi { get; set; } = new();

        /// <summary>
        /// Gets or sets the casing style for routes.
        /// </summary>
        public CasingStyle CaseStyle { get; set; } = CasingStyle.KebabCase;

        /// <summary>
        /// Gets or sets a custom case converter.
        /// </summary>
        public ICaseConverter? CaseConverter { get; set; }

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
            var cloned = (RouteConventionOptions)MemberwiseClone();
            cloned.RazorPages = (RazorPagesRouteOptions)RazorPages.Clone();
            cloned.Controllers = (ControllerRouteOptions)Controllers.Clone();
            cloned.MinimalApi = (MinimalApiRouteOptions)MinimalApi.Clone();
            cloned.Hooks = (RouteConventionHooks)Hooks.Clone();

            return cloned;
        }

        /// <summary>
        /// Gets the case converter for this configuration.
        /// </summary>
        internal ICaseConverter GetCaseConverter()
        {
            var defaultCaseConverter = CaseConverterFactory.CreateKebabCase();
            return CaseConverter ?? CaseStyle switch
            {
                CasingStyle.KebabCase => defaultCaseConverter,
                CasingStyle.SnakeCase => CaseConverterFactory.CreateSnakeCase(),
                CasingStyle.CamelCase => CaseConverterFactory.CreateCamelCase(),
                CasingStyle.PascalCase => CaseConverterFactory.CreatePascalCase(),
                _ => defaultCaseConverter,
            };
        }
    }
}
