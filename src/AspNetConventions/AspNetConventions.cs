using System;
using AspNetConventions.Configuration;
using AspNetConventions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetConventions
{
    /// <summary>
    /// Provides extension methods for configuring standardized ASP.NET Core conventions and services for MVC and
    /// Minimal API applications.
    /// </summary>
    public static class AspNetConventions
    {
        /// <summary>
        /// Adds AspNetConventions to the MVC services with optional configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure.</param>
        /// <param name="configure">An optional action to configure conventions. If null, default settings are used.</param>
        /// <returns>The same <see cref="IMvcBuilder"/> instance so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        public static IMvcBuilder AddAspNetConventions(
            this IMvcBuilder builder,
            Action<AspNetConventionOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Add options to the service collection
            builder.Services
                .AddOptions<AspNetConventionOptions>()
                .ValidateOnStart();

            // Apply user configuration if provided
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }

            // Add options validator
            builder.Services.AddSingleton<
                IValidateOptions<AspNetConventionOptions>,
                AspNetConventionOptionsValidator>();

            // Add JSON serialization settings
            builder.AddMvcJsonOptions();

            // Add MVC and Razor Page conventions
            builder.AddRazorPageConventions();
            builder.AddMvcConventions();

            return builder;
        }

        /// <summary>
        /// Configures the application to use AspNetConventions for Minimal APIs.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
        /// <param name="configure">An optional action to configure conventions. If null, default settings are used.</param>
        /// <returns>The <see cref="WebApplication"/> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is null.</exception>
        public static WebApplication UseAspNetConventions(
            this WebApplication app,
            Action<AspNetConventionOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(app);

            // Build and validate options
            var options = app.Services.BuildAspNetConventionOptions(configure);

            // Apply JSON serialization settings
            app.UseHttpJsonOptions(options.Json);

            // Apply endpoint conventions
            app.UseEndpointConventions(options);

            // Apply exception handling middleware
            app.UseExceptionHandlingMiddleware(options);

            // Apply response formatting middleware
            app.UseResponseFormattingMiddleware(options);

            return app;
        }
    }
}
