namespace AspNetConventions.Configuration
{
    public class RazorPageOptions
    {
        public bool IsEnabled { get; set; } = true;

        public bool TransformPageRoutes { get; set; } = true;

        public bool TransformRouteParameters { get; set; } = true;

        public bool TransformBindingNames { get; set; }

        public bool TransformHandlerNames { get; set; }

        public bool PreserveAreas { get; set; }
    }
}
