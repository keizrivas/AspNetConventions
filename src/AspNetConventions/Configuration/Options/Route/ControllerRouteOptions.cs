using System;
using System.Collections.Generic;

namespace AspNetConventions.Configuration.Options.Route
{
    public class ControllerRouteOptions: ICloneable
    {
        /// <summary>
        /// Enable route transformations for MVC controllers
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public bool TransformParameterNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform token names.
        /// </summary>
        public bool TransformRouteTokens { get; set; } = true;

        public bool PreserveExplicitBindingNames { get; set; }

        public HashSet<string> RemoveActionPrefixes { get; private set; } = [];

        public HashSet<string> RemoveActionSuffixes { get; private set; } = [];

        /// <summary>
        /// Controllers to exclude from route transformation (by name, without suffix)
        /// </summary>
        public HashSet<string> ExcludeControllers { get; private set; } = [];

        /// <summary>
        /// Actions to exclude from route transformation
        /// </summary>
        public HashSet<string> ExcludeActions { get; private set; } = [];

        /// <summary>
        /// Areas to exclude from route transformation
        /// </summary>
        public HashSet<string> ExcludeAreas { get; private set; } = [];

        public object Clone()
        {
            var cloned = (MvcRouteOptions)MemberwiseClone();
            cloned.RemoveActionPrefixes = [.. RemoveActionPrefixes];
            cloned.RemoveActionSuffixes = [.. RemoveActionSuffixes];
            cloned.ExcludeControllers = [..ExcludeControllers];
            cloned.ExcludeActions = [..ExcludeActions];
            cloned.ExcludeAreas = [..ExcludeAreas];
            return cloned;
        }
    }
}
