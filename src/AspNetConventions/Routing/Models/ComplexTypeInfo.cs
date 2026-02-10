using System;
using System.Collections.Generic;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Contains cached information about a complex type for binding and transformation purposes.
    /// </summary>
    /// <remarks>
    /// This class stores metadata about user-defined complex types that have been analyzed
    /// for property binding and naming convention transformation.
    /// </remarks>
    internal sealed class ComplexTypeInfo
    {
        /// <summary>
        /// Gets or sets the complex type being cached.
        /// </summary>
        public required Type Type { get; set; }

        /// <summary>
        /// Gets or sets the set of public instance property names for the complex type.
        /// </summary>
        /// <value>A hash set containing the names of all bindable properties in the type.</value>
        public required HashSet<string> PropertyNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this type serves as a Razor Page container.
        /// </summary>
        /// <value>true if the type is a Razor PageModel or similar container; otherwise, false.</value>
        /// <remarks>
        /// This flag is used to identify Razor Page scenarios where properties might be
        /// processed differently due to the dual nature of Razor Page parameter binding.
        /// </remarks>
        public bool RazorPageContainer { get; set; }
    }
}
