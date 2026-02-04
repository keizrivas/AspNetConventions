using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetConventions.Core.Enums;

namespace AspNetConventions.Routing.Models
{
    public readonly record struct RouteIdentity(
        RouteSourceKind Kind,
        string Id);

}
