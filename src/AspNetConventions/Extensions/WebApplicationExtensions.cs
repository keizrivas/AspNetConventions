namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Extension methods for WebApplication to configure middleware.
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Adds response formatting middleware to the pipeline.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <param name="options">The convention options.</param>
        /// <returns>The web application for method chaining.</returns>
        /// <remarks>
        /// This middleware should be placed after authentication/authorization
        /// but before endpoint routing.
        /// </remarks>
        //public static WebApplication UseResponseFormatting(
        //    this WebApplication app,
        //    AspNetConventionOptions options)
        //{
        //    ArgumentNullException.ThrowIfNull(app);
        //    ArgumentNullException.ThrowIfNull(options);

        //    if (!options.Response.IsEnabled)
        //        return app;

        //    // Register formatters if not already registered
        //    if (!app.Services.Any(s => s.ServiceType == typeof(IEnumerable<IResponseFormatter>)))
        //    {
        //        // No custom formatters registered, use empty list
        //        app.Use(async (context, next) =>
        //        {
        //            var middleware = new ResponseFormattingMiddleware(
        //                next,
        //                options.Response.GetResponseBuilder(),
        //                Enumerable.Empty<IResponseFormatter>(),
        //                options,
        //                context.RequestServices.GetRequiredService<ILogger<ResponseFormattingMiddleware>>());

        //            await middleware.InvokeAsync(context);
        //        });
        //    }
        //    else
        //    {
        //        app.UseMiddleware<ResponseFormattingMiddleware>(options);
        //    }

        //    return app;
        //}

        /// <summary>
        /// Adds JSON serialization middleware to the pipeline.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <param name="options">The convention options.</param>
        /// <returns>The web application for method chaining.</returns>
        /// <remarks>
        /// This middleware should be placed after response formatting
        /// to ensure JSON conventions are applied consistently.
        /// </remarks>
        /// <example>
        /// <code>
        /// var app = builder.Build();
        /// 
        /// app.UseResponseFormatting(options);
        /// app.UseJsonSerialization(options);
        /// 
        /// app.MapControllers();
        /// app.Run();
        /// </code>
        /// </example>
        //public static WebApplication UseJsonSerialization(
        //    this WebApplication app,
        //    AspNetConventionOptions options)
        //{
        //    ArgumentNullException.ThrowIfNull(app);
        //    ArgumentNullException.ThrowIfNull(options);

        //    var jsonOptions = options.Json.BuildSerializerOptions();

        //    app.Use(async (context, next) =>
        //    {
        //        var middleware = new JsonSerializationMiddleware(
        //            next,
        //            jsonOptions,
        //            context.RequestServices.GetRequiredService<ILogger<JsonSerializationMiddleware>>());

        //        await middleware.InvokeAsync(context);
        //    });

        //    return app;
        //}
    }
}
