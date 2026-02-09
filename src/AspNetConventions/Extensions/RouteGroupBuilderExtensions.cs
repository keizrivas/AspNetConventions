using System;
using System.Xml.Linq;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Handlers;
using AspNetConventions.Routing.Transformation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Extension methods for IEndpointRouteBuilder.
    /// </summary>
    internal static partial class RouteGroupBuilderExtensions
    {
        /// <summary>
        /// Applies route conventions to endpoints in the route group.
        /// </summary>
        /// <param name="group">The route group builder to apply conventions to.</param>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="group"/> or <paramref name="options"/> is null.</exception>
        /// <remarks>
        /// This method applies route transformations to all endpoints in the group when route conventions are enabled
        /// for Minimal APIs. It uses an <see cref="EndpointTransformer"/> to modify route patterns according
        /// to the configured naming conventions.
        /// </remarks>
        internal static void UseEndpointConventions(
            this RouteGroupBuilder group,
            AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(group);
            ArgumentNullException.ThrowIfNull(options);

            if (!options.Route.IsEnabled || !options.Route.MinimalApi.IsEnabled)
            {
                return;
            }

            var transformer = new EndpointTransformer(options);
            ((IEndpointConventionBuilder)group).Finally(builder =>
            {
                if (builder is not RouteEndpointBuilder routeEndpointBuilder)
                {
                    return;
                }

                transformer.TransformRoutePattern(routeEndpointBuilder);
            });
        }

        /// <summary>
        /// Configures exception handling middleware for the web application.
        /// </summary>
        /// <param name="app">The web application to configure exception handling for.</param>
        /// <param name="options">The AspNetConventions configuration options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or <paramref name="options"/> is null.</exception>
        /// <remarks>
        /// This method configures the ASP.NET Core exception handling middleware to use AspNetConventions'
        /// standardized exception handling. It creates a <see cref="GlobalExceptionHandler"/> to process
        /// exceptions and return formatted error responses according to the configured options.
        /// </remarks>
        internal static void UseExceptionHandlingMiddleware(
            this WebApplication app,
            AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(options);

            // Get services
            var logger = app.Services.GetRequiredService<ILogger<GlobalExceptionHandler>>();
            var serializer = app.Services.GetOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>()
                .Value.SerializerOptions;


            // Create the exception handler
            var writer = app.Services.GetRequiredService<IExceptionResponseWriter>()
                ?? new ExceptionResponseWriter(Options.Create(options), logger);

            var handler = new GlobalExceptionHandler(writer, serializer);

            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    // Get the exception from the feature
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (exceptionFeature == null)
                    {
                        // No exception found, set default status code
                        context.Response.StatusCode = (int)options.Response.ErrorResponse.DefaultStatusCode;
                        return;
                    }

                    await handler.TryHandleAsync(
                        context,
                        exceptionFeature.Error,
                        context.RequestAborted).ConfigureAwait(false);
                });
            });
        }

        /// <summary>
        /// Applies HTTP JSON options to the web application's existing JSON configuration.
        /// </summary>
        /// <param name="app">The web application to configure JSON options for.</param>
        /// <param name="options">The JSON serialization options to apply.</param>
        /// <returns>The same <see cref="WebApplication"/> instance for method chaining.</returns>
        /// <remarks>
        /// This method applies AspNetConventions JSON serialization options to the existing HTTP JSON options
        /// service. If the JSON options service is not available or options are disabled, the application is
        /// returned unchanged.
        /// </remarks>
        internal static WebApplication UseHttpJsonOptions(this WebApplication app, JsonSerializationOptions options)
        {
            // Try to get the configured json options service
            var httpJsonOptions = app.Services.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()?.Value;
            if (!options.IsEnabled || httpJsonOptions == null)
            {
                return app;
            }

            // Apply json serializer options to existing service
            var serializerOptions = options.BuildSerializerOptions();
            httpJsonOptions.SerializerOptions.ApplyFrom(serializerOptions);

            return app;
        }
    }
}
