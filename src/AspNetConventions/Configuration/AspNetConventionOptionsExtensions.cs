using System;
using AspNetConventions.Common.Enums;
using AspNetConventions.ResponseFormatting.Abstractions;
using AspNetConventions.ResponseFormatting.Enums;
using static AspNetConventions.Common.Hooks.ExceptionHandlingHooks;

namespace AspNetConventions.Configuration
{
    /// <summary>
    /// Provides fluent extension methods for configuring <see cref="AspNetConventionOptions"/>.
    /// </summary>
    public static class AspNetConventionOptionsExtensions
    {
        /// <summary>
        /// Configures routes to use "kebab-case" naming convention.
        /// </summary>
        public static AspNetConventionOptions UseKebabCaseRoutes(this AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Route);

            options.Route.CaseStyle = CasingStyle.KebabCase;
            return options;
        }

        /// <summary>
        /// Configures routes to use "snake_case" naming convention.
        /// </summary>
        public static AspNetConventionOptions UseSnakeCaseRoutes(this AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Route);

            options.Route.CaseStyle = CasingStyle.SnakeCase;
            return options;
        }

        /// <summary>
        /// Configures routes to use "camelCase" naming convention.
        /// </summary>
        public static AspNetConventionOptions UseCamelCaseRoutes(this AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Route);

            options.Route.CaseStyle = CasingStyle.CamelCase;
            return options;
        }

        /// <summary>
        /// Configures JSON serialization to use "camelCase" naming convention.
        /// </summary>
        public static AspNetConventionOptions UseCamelCaseJson(this AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Json);

            options.Json.CaseStyle = CasingStyle.CamelCase;
            return options;
        }

        /// <summary>
        /// Configures JSON serialization to use "snake_case" naming convention.
        /// </summary>
        public static AspNetConventionOptions UseSnakeCaseJson(this AspNetConventionOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Json);

            options.Json.CaseStyle = CasingStyle.SnakeCase;
            return options;
        }

        /// <summary>
        /// Configures routes with custom settings.
        /// </summary>
        public static AspNetConventionOptions ConfigureRoutes(
            this AspNetConventionOptions options,
            Action<RouteConventionOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(configure);

            configure(options.Route);
            return options;
        }

        /// <summary>
        /// Configures response formatting with custom settings.
        /// </summary>
        public static AspNetConventionOptions ConfigureResponses(
            this AspNetConventionOptions options,
            Action<ResponseFormattingOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(configure);

            configure(options.Response);
            return options;
        }

        /// <summary>
        /// Configures JSON serialization with custom settings.
        /// </summary>
        public static AspNetConventionOptions ConfigureJson(
            this AspNetConventionOptions options,
            Action<JsonSerializationOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(configure);

            configure(options.Json);
            return options;
        }

        /// <summary>
        /// Configures exception handling with custom settings.
        /// </summary>
        public static AspNetConventionOptions ConfigureExceptionHandling(
            this AspNetConventionOptions options,
            Action<ExceptionHandlingOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(configure);

            configure(options.ExceptionHandling);
            return options;
        }

        /// <summary>
        /// Registers a custom error handler that is invoked when convention errors occur.
        /// </summary>
        public static AspNetConventionOptions OnError(
            this AspNetConventionOptions options,
            TryHandleCallbackAsync handler)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(handler);

            options.ExceptionHandling.Hooks.TryHandleAsync = handler;
            return options;
        }

        /// <summary>
        /// Registers a custom response builder.
        /// </summary>
        public static AspNetConventionOptions WithCustomResponseBuilder(
            this AspNetConventionOptions options,
            IResponseBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Response);

            options.Response.CustomResponseBuilder = builder;
            options.Response.Style = ResponseStyle.Custom;
            return options;
        }
    }
}
