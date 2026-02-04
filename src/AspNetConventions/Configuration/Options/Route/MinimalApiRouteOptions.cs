using System;
using System.Collections.Generic;

namespace AspNetConventions.Configuration.Options.Route
{
    /// <summary>
    /// Provides configuration options for Minimal API route naming conventions.
    /// </summary>
    /// <remarks>
    /// These options control how Minimal API endpoints routes are transformed and named, including parameter
    /// transformations, route pattern exclusions, and tag-based exclusions.
    /// </remarks>
    public class MinimalApiRouteOptions: ICloneable
    {
        /// <summary>
        /// Gets or sets whether route transformations are enabled for Minimal API endpoints.
        /// </summary>
        /// <value>Default is true. When disabled, Minimal API routes will not be transformed.</value>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform route parameters in Minimal API endpoints.
        /// </summary>
        /// <value>Default is true. When enabled, route parameters will be converted to the configured case style.</value>
        public bool TransformRouteParameters { get; set; }

        /// <summary>
        /// Gets the collection of route patterns to exclude from transformation.
        /// </summary>
        /// <value>Collection of route patterns (supports wildcards) that will not have their routes transformed.</value>
        public HashSet<string> ExcludeRoutePatterns { get; private set; } = [];

        /// <summary>
        /// Gets the collection of endpoint tags to exclude from transformation.
        /// </summary>
        /// <value>Collection of endpoint tags that will not have their routes transformed.</value>
        public HashSet<string> ExcludeTags { get; private set; } = [];

        /// <summary>
        /// Creates a deep clone of the <see cref="MinimalApiRouteOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="MinimalApiRouteOptions"/> instance with all collections cloned.</returns>
        public object Clone()
        {
            var cloned = (MinimalApiRouteOptions)MemberwiseClone();
            cloned.ExcludeRoutePatterns = [..ExcludeRoutePatterns];
            cloned.ExcludeTags = [..ExcludeTags];
            return cloned;
        }
    }
}
