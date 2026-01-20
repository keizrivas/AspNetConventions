using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
