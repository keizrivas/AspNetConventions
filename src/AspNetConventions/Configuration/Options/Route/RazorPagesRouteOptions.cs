using System;
using System.Collections.Generic;

namespace AspNetConventions.Configuration.Options.Route
{
    /// <summary>
    /// Provides configuration options for Razor Pages route naming conventions.
    /// </summary>
    /// <remarks>
    /// These options control how Razor Pages routes are transformed and named, including parameter and property
    /// name transformations, folder and page exclusions, and binding name preservation.
    /// </remarks>
    public class RazorPagesRouteOptions: ICloneable
    {
        /// <summary>
        /// Gets or sets whether route transformations are enabled for Razor Pages.
        /// </summary>
        /// <value>Default is true. When disabled, Razor Pages routes will not be transformed.</value>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to transform parameter names in Razor Pages routes.
        /// </summary>
        /// <value>Default is true. When enabled, route parameters will be converted to the configured case style.</value>
        public bool TransformParameterNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform property names used in page model binding.
        /// </summary>
        /// <value>Default is true. When enabled, property names in page models will be converted to the configured case style.</value>
        public bool TransformPropertyNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to preserve explicitly named parameters.
        /// </summary>
        /// <value>Default is false. When true, parameters with explicit binding name are not transformed.</value>
        public bool PreserveExplicitBindingNames { get; set; }

        /// <summary>
        /// Gets the collection of folders to exclude from route transformation.
        /// </summary>
        /// <value>Collection of folder names that will not have their routes transformed.</value>
        public HashSet<string> ExcludeFolders { get; private set; } = [];

        /// <summary>
        /// Gets the collection of page names to exclude from route transformation.
        /// </summary>
        /// <value>Collection of page names (without extension) that will not have their routes transformed.</value>
        public HashSet<string> ExcludePages { get; private set; } = [];

        /// <summary>
        /// Creates a deep clone of the <see cref="RazorPagesRouteOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="RazorPagesRouteOptions"/> instance with all collections cloned.</returns>
        public object Clone()
        {
            var cloned = (RazorPagesRouteOptions)MemberwiseClone();
            cloned.ExcludeFolders = [..ExcludeFolders];
            cloned.ExcludePages = [..ExcludePages];
            return cloned;
        }
    }
}
