namespace AspNetConventions.Configuration
{
    public class ControllerOptions
    {
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to transform controller names.
        /// </summary>
        public bool TransformRouteTokens { get; set; } = true;

        public bool TransformModelNames { get; set; }
    }
}
