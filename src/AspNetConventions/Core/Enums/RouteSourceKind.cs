namespace AspNetConventions.Core.Enums
{
    /// <summary>
    /// Specifies the type of source that defines an HTTP route in an ASP.NET Core application.
    /// </summary>
    /// <remarks>Use this enumeration to distinguish between routes defined by MVC controller actions, Razor
    /// Pages, or minimal APIs.</remarks>
    public enum RouteSourceKind
    {
        /// <summary>
        /// MVC Controller action endpoint type.
        /// </summary>
        MvcAction,

        /// <summary>
        /// Razor Pages endpoint type.
        /// </summary>
        RazorPage,

        /// <summary>
        /// Minimal API endpoint type.
        /// </summary>
        MinimalApi
    }
}
