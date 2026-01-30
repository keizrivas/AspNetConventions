using System;
using System.Collections.Generic;
using System.Linq;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Handlers;
using AspNetConventions.Routing.Transformation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        /// Applies conventions to endpoints.
        /// </summary>
        internal static void UseEndpointConventions(
            this RouteGroupBuilder group,
            AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(group);
            ArgumentNullException.ThrowIfNull(options);

            if (!options.Route.IsEnabled)
            {
                return;
            }

            var transformer = new EndpointTransformer(options);
            var newDataSources = new List<EndpointDataSourceWrapper>();

            ((IEndpointConventionBuilder)group).Finally(builder =>
            {
                if (builder is RouteEndpointBuilder routeBuilder)
                {
                    transformer.TransformEndpoint(routeBuilder);

                    //var originalPattern = routeBuilder.RoutePattern.RawText;
                    //if (string.IsNullOrEmpty(originalPattern))
                    //    return;

                    //// Transform the route pattern
                    //var transformedRoute = transformer.TransformEndpoint(routeBuilder);

                    //if (transformedRoute != originalPattern)
                    //{
                    //    // Create new route pattern with transformed path
                    //    var newPattern = RoutePatternFactory.Parse(transformedRoute);
                    //    routeBuilder.RoutePattern = newPattern;
                    //}

                    //// Transform parameter names for model binding
                    //if (options.Route.TransformParametersToKebabCase)
                    //{
                    //    TransformParameterMetadata(routeBuilder, transformer);
                    //}
                }
            });

        }

        internal static void UseExceptionHandlingMiddleware(
            this WebApplication app,
            AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(options);

            // Get services
            var logger = app.Services.GetRequiredService<ILogger<MinimalApiExceptionHandler>>();
            var serializer = app.Services.GetOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>()
                .Value.SerializerOptions ?? options.Json.BuildSerializerOptions();

            // Create the exception handler
            var handler = new MinimalApiExceptionHandler(options, serializer, logger);

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
        /// Adds response formatting middleware.
        /// </summary>
        internal static void UseResponseFormattingMiddleware(
            this WebApplication app,
            AspNetConventionOptions options)
        {
            if (!options.Response.IsEnabled)
                return;

            // Response formatting is currently handled by the output formatter
            // This method is here for future middleware needs or custom response pipelines

            // Future features could include:
            // - Response compression
            // - Response caching headers
            // - CORS headers
            // - Custom content negotiation
        }

        internal static IServiceCollection AddHttpJsonOptions(this IServiceCollection services)
        {
            // Configure MVC JSON options
            services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>(serviceProvider =>
            {
                // Build and validate options
                var options = serviceProvider.BuildAspNetConventionOptions();

                return new ConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>(jsonOptions =>
                {
                    var serializerOptions = options.Json.BuildSerializerOptions();
                    jsonOptions.SerializerOptions.ApplyOptions(serializerOptions);
                });
            });

            return services;
        }

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
            httpJsonOptions.SerializerOptions.ApplyOptions(serializerOptions);

            return app;
        }
    }
}
