using System;
using System.Collections.Generic;

namespace AspNetConventions.Routing.Models
{
    /// <summary>
    /// Information about a cached complex type
    /// </summary>
    internal class ComplexTypeInfo
    {
        public required Type Type { get; set; }

        public required HashSet<string> PropertyNames { get; set; }

        public bool RazorPageContainer { get; set; }
    }
}
