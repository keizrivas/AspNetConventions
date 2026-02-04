namespace AspNetConventions.Core.Enums
{
    /// <summary>
    /// Specifies the type of endpoint exposed by an ASP.NET Core application.
    /// </summary>
    /// <remarks>Use this enumeration to identify the kind of endpoint being configured or processed.
    /// The value <see cref="Unknown"/> indicates that the endpoint type is not recognized or has not been
    /// specified.</remarks>
    public enum EndpointType
    {
        /// <summary>
        /// Unknown or unrecognized endpoint type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Razor Pages endpoint type.
        /// </summary>
        RazorPage,

        /// <summary>
        /// MVC Controller action endpoint type.
        /// </summary>
        MvcAction,

        /// <summary>
        /// Minimal API endpoint type.
        /// </summary>
        MinimalApi,

        /// <summary>
        /// Blazor component endpoint type.
        /// </summary>
        Blazor,

        /// <summary>
        /// Health check endpoint type.
        /// </summary>
        HealthCheck,

        /// <summary>
        /// Static file serving endpoint type.
        /// </summary>
        StaticFiles,

        /// <summary>
        /// SignalR hub endpoint type.
        /// </summary>
        SignalR
    }
}
