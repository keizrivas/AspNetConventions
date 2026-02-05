using System;
using System.Collections.Generic;

namespace AspNetConventions.Configuration.Options.Route
{
    /// <summary>
    /// Provides configuration options for MVC controller route naming conventions.
    /// </summary>
    /// <remarks>
    /// These options control how MVC controller routes are transformed and named, including parameter name
    /// transformations, action naming conventions, and exclusion lists for controllers, actions, and areas.
    /// </remarks>
    public class ControllerRouteOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether route transformations are enabled for MVC controllers.
        /// </summary>
        /// <value>Default is true. When disabled, MVC controller routes will not be transformed.</value>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform parameter names in MVC controller routes.
        /// </summary>
        /// <value>Default is true. When enabled, route parameters will be converted to the configured case style.</value>
        public bool TransformParameterNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform route tokens in MVC controller routes.
        /// </summary>
        /// <value>Default is true. When enabled, route segments will be converted to the configured case style.</value>
        public bool TransformRouteTokens { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to preserve explicit binding names in route transformations.
        /// </summary>
        /// <value>Default is false. When true, explicitly defined binding names will not be transformed.</value>
        public bool PreserveExplicitBindingNames { get; set; }

        /// <summary>
        /// Gets the collection of action prefixes to remove from route names.
        /// </summary>
        /// <value>Collection of string prefixes that will be stripped from action names when generating routes.</value>
        public HashSet<string> RemoveActionPrefixes { get; private set; } = [];

        /// <summary>
        /// Gets the collection of action suffixes to remove from route names.
        /// </summary>
        /// <value>Collection of string suffixes that will be stripped from action names when generating routes.</value>
        public HashSet<string> RemoveActionSuffixes { get; private set; } = [];

        /// <summary>
        /// Gets the collection of controllers to exclude from route transformation.
        /// </summary>
        /// <value>Collection of controller names (without "Controller" suffix) that will not have their routes transformed.</value>
        public HashSet<string> ExcludeControllers { get; private set; } = [];

        /// <summary>
        /// Gets the collection of actions to exclude from route transformation.
        /// </summary>
        /// <value>Collection of action method names that will not have their routes transformed.</value>
        public HashSet<string> ExcludeActions { get; private set; } = [];

        /// <summary>
        /// Gets the collection of areas to exclude from route transformation.
        /// </summary>
        /// <value>Collection of area names that will not have their routes transformed.</value>
        public HashSet<string> ExcludeAreas { get; private set; } = [];

        /// <summary>
        /// Creates a deep clone of the <see cref="ControllerRouteOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ControllerRouteOptions"/> instance with all collections cloned.</returns>
        public object Clone()
        {
            var cloned = (ControllerRouteOptions)MemberwiseClone();
            cloned.RemoveActionPrefixes = [.. RemoveActionPrefixes];
            cloned.RemoveActionSuffixes = [.. RemoveActionSuffixes];
            cloned.ExcludeControllers = [.. ExcludeControllers];
            cloned.ExcludeActions = [.. ExcludeActions];
            cloned.ExcludeAreas = [.. ExcludeAreas];
            return cloned;
        }
    }
}
