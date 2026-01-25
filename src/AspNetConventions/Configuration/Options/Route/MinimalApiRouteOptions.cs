using System;
using System.Collections.Generic;

namespace AspNetConventions.Configuration.Options.Route
{
    public class MinimalApiRouteOptions: ICloneable
    {
        /// <summary>
        /// Enable route transformations for Minimal API endpoints
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public bool TransformRouteParameters { get; set; }

        /// <summary>
        /// Route patterns to exclude from transformation (supports wildcards)
        /// </summary>
        public HashSet<string> ExcludeRoutePatterns { get; private set; } = [];

        /// <summary>
        /// Endpoint tags to exclude from transformation
        /// </summary>
        public HashSet<string> ExcludeTags { get; private set; } = [];

        public object Clone()
        {
            var cloned = (MinimalApiRouteOptions)MemberwiseClone();
            cloned.ExcludeRoutePatterns = [..ExcludeRoutePatterns];
            cloned.ExcludeTags = [..ExcludeTags];
            return cloned;
        }
    }
}
