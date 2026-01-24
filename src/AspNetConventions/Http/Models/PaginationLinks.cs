using System;

namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Links for navigating paginated results.
    /// </summary>
    public sealed class PaginationLinks
    {
        public Uri? FirstPageUrl { get; set; }
        public Uri? LastPageUrl { get; set; }
        public Uri? NextPageUrl { get; set; }
        public Uri? PreviousPageUrl { get; set; }
    }
}
