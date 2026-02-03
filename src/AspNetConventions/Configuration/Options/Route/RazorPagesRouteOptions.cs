using System;
using System.Collections.Generic;

namespace AspNetConventions.Configuration.Options.Route
{
    public class RazorPagesRouteOptions: ICloneable
    {
        public bool IsEnabled { get; set; } = true;
        
        public bool TransformParameterNames { get; set; } = true;

        /// <summary>
        /// Transform property names used in page model binding
        /// </summary>
        public bool TransformPropertyNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to preserve explicitly named parameters.
        /// When true, parameters with explicit [Bind(Prefix = "...")]/[BindProperty(Name = "...")] are not transformed.
        /// </summary>
        public bool PreserveExplicitBindingNames { get; set; }

        /// <summary>
        /// Folders to exclude from route transformation
        /// </summary>
        public HashSet<string> ExcludeFolders { get; private set; } = [];

        /// <summary>
        /// Page names to exclude from route transformation
        /// </summary>
        public HashSet<string> ExcludePages { get; private set; } = [];

        //public bool TransformBindingNames { get; set; }

        //public bool TransformHandlerNames { get; set; }

        //public bool PreserveAreas { get; set; }

        public object Clone()
        {
            var cloned = (RazorPagesRouteOptions)MemberwiseClone();
            cloned.ExcludeFolders = [..ExcludeFolders];
            cloned.ExcludePages = [..ExcludePages];
            return cloned;
        }
    }
}
