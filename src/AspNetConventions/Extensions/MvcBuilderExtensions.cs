using System.Threading;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Filters;
using AspNetConventions.ExceptionHandling.Handlers;
using AspNetConventions.Routing.Conventions;
using AspNetConventions.Routing.Providers;
using AspNetConventions.Routing.Transformation;
using AspNetConventions.Serialization.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring MVC and Razor Pages with standardized 
    /// conventions, filters, and JSON serialization options.
    /// </summary>
    internal static partial class MvcBuilderExtensions
    {
        /// <summary>
        /// Adds standard MVC conventions, filters, and formatters to the specified MVC builder.
        /// </summary>
        /// <remarks>This method registers custom controller conventions, global exception filters and a
        /// response wrapping JSON formatter.</remarks>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure.</param>
        /// <returns>The same <see cref="IMvcBuilder"/> instance so that additional calls can be chained.</returns>
        internal static IMvcBuilder AddMvcConventions(this IMvcBuilder builder)
        {
            // Scoped filter
            builder.Services.AddScoped<ControllerExceptionFilter>();
            builder.Services.AddSingleton<IInvalidModelStateFactory, ControllerInvalidModelStateFactory>();

            // Register convention
            builder.Services.AddSingleton<RouteControllerConvention>();
            builder.Services.AddSingleton<IValueProviderFactory, QueryValueProviderFactory>();
            builder.Services.AddSingleton<IOutboundParameterTransformer, RouteTokenTransformer>();
            builder.Services.AddSingleton<IBindingMetadataProvider, ComplexTypeBindingMetadataProvider>();

            // Add Mvc conventions
            builder.Services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>(serviceProvider =>
            {
                // Get required services
                var options = serviceProvider.GetOptions<AspNetConventionOptions>();
                var controllerConvention = serviceProvider.GetRequiredService<RouteControllerConvention>();
                var routeTokenTransformer = serviceProvider.GetRequiredService<IOutboundParameterTransformer>();
                var jsonOptions = serviceProvider.GetOptions<Microsoft.AspNetCore.Mvc.JsonOptions>();
                var metadataProvider = serviceProvider.GetRequiredService<IBindingMetadataProvider>();
                var valueProviderFactory = serviceProvider.GetRequiredService<IValueProviderFactory>();

                return new ConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>(mvcOptions =>
                {
                    // Add endpoint conventions
                    mvcOptions.Conventions.Add(controllerConvention);
                    mvcOptions.Conventions.Add(new RouteTokenTransformerConvention(routeTokenTransformer));

                    // Register response collection adapters
                    foreach (var adapters in options.Value.Response.ResponseCollectionAdapters)
                    {
                        builder.Services.AddSingleton<IResponseCollectionAdapter>(adapters);
                    }

                    // Add response formatter
                    mvcOptions.OutputFormatters.Insert(0,
                        new ResponseJsonFormatter(
                            options,
                            jsonOptions.Value.JsonSerializerOptions
                        )
                    );

                    // Add custom value provider
                    mvcOptions.ValueProviderFactories.Insert(0, valueProviderFactory);

                    // Add exception handling filter
                    mvcOptions.Filters.Add<ControllerExceptionFilter>();
                    mvcOptions.Filters.Add<ExceptionStatusCodeResultFilter>();

                    // Add model metadata provider
                    mvcOptions.ModelMetadataDetailsProviders.Insert(0, metadataProvider);
                    //mvcOptions.ModelBinderProviders.Insert(0, binderProvider);
                });
            });

            // Configure invalid model state response factory
            builder.Services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>>(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<IInvalidModelStateFactory>();
                return new ConfigureOptions<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(apiOptions =>
                {
                    apiOptions.InvalidModelStateResponseFactory = context =>
                    {
                        return factory.Create(context);
                    };
                });
            });

            return builder;
        }

        /// <summary>
        /// Configures global exception handling for MVC controllers by adding a custom exception response writer 
        /// and setting up the exception handling middleware.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure.</param>
        /// <returns>The same <see cref="IMvcBuilder"/> instance so that additional calls can be chained.</returns>
        internal static IMvcBuilder AddExceptionResponseWriter(this IMvcBuilder builder)
        {
            // Exception hanlder
            builder.Services.AddSingleton<IExceptionResponseWriter, ExceptionResponseWriter>();

            // Configure exception handling middleware
            builder.Services.AddExceptionHandler(options =>
            {
                options.ExceptionHandler = async context =>
                {
                    var writer = context.RequestServices
                        .GetRequiredService<IExceptionResponseWriter>();

                    var serializerOptions = context.RequestServices
                        .GetOptions<Microsoft.AspNetCore.Mvc.JsonOptions>()
                        .Value.JsonSerializerOptions;

                    var exception = context.Features
                        .Get<IExceptionHandlerFeature>()?.Error;

                    if (exception == null)
                    {
                        return;

                        // Log the exception
                        //var logger = context.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
                        //logger.LogError(exception, "An unhandled exception occurred while processing the request.");
                    }

                    await writer
                        .WithSerializerOptions(serializerOptions)
                        .WriteResponseAsync(context, exception, CancellationToken.None)
                        .ConfigureAwait(false);
                };
            });

            return builder;
        }

        /// <summary>
        /// Adds Razor Page conventions and exception handling filters to the MVC builder for enhanced Razor Pages
        /// configuration.
        /// </summary>
        /// <remarks>This method registers custom conventions for Razor Page routing and parameter
        /// handling and exception filters for Razor Pages.</remarks>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure.</param>
        /// <returns>The same <see cref="IMvcBuilder"/> instance so that additional calls can be chained.</returns>
        internal static IMvcBuilder AddRazorPageConventions(this IMvcBuilder builder)
        {
            // Scoped filter
            builder.Services.AddScoped<RazorPageExceptionFilter>();

            // Register conventions
            builder.Services.AddSingleton<RazorPageRouteConvention>();
            builder.Services.AddSingleton<RazorPageParameterConvention>();

            // Add razor pages conventions
            builder.Services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions>>(serviceProvider =>
            {
                var routeConvention = serviceProvider.GetRequiredService<RazorPageRouteConvention>();
                var parameterConvention = serviceProvider.GetRequiredService<RazorPageParameterConvention>();

                return new ConfigureOptions<Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions>(razor =>
                {
                    // Route conventions
                    razor.Conventions.Insert(0, routeConvention);

                    // Route parameter conventions
                    razor.Conventions.Insert(1, parameterConvention);

                    // Add exception handling filter
                    razor.Conventions.ConfigureFilter(new RazorPageExceptionFilterFactory());

                });
            });

            return builder;
        }

        /// <summary>
        /// Configures MVC to use custom JSON serialization options based on application conventions.
        /// </summary>
        /// <remarks>
        /// This method applies JSON serializer settings according to the application's
        /// configured conventions. It should be called before adding controllers or other MVC services that depend on
        /// JSON serialization.
        /// </remarks>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure.</param>
        /// <returns>The same <see cref="IMvcBuilder"/> instance so that additional calls can be chained.</returns>
        internal static IMvcBuilder AddMvcJsonOptions(this IMvcBuilder builder)
        {
            // Configure MVC JSON options
            builder.Services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>(serviceProvider =>
            {
                var options = serviceProvider.GetAspNetConventionOptions();

                return new ConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>(jsonOptions =>
                {
                    // Get and validate json options
                    if (!options.Json.IsEnabled)
                    {
                        return;
                    }

                    // Set json serializer options
                    var serializerOptions = options.Json.BuildSerializerOptions();
                    jsonOptions.JsonSerializerOptions.ApplyFrom(serializerOptions);
                });
            });

            return builder;
        }
    }
}
