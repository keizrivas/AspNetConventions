namespace AspNetConventions.Common.Enums
{
    /// <summary>
    /// Specifies the type of endpoint exposed by an ASP.NET Core application.
    /// </summary>
    /// <remarks>Use this enumeration to identify the kind of endpoint being configured or processed.
    /// The value <see cref="EndpointType.Unknown"/> indicates that the endpoint type is not recognized or has not been
    /// specified.</remarks>
    public enum EndpointType
    {
        Unknown,
        RazorPage,
        MvcController,
        MinimalApi,
        Blazor,
        HealthCheck,
        StaticFiles,
        SignalR
    }
}
