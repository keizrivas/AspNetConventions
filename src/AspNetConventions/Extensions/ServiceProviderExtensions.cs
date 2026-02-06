using System;
using AspNetConventions.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for service provider operations related to AspNetConventions configuration.
    /// </summary>
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        /// Builds a new instance of <see cref="AspNetConventionOptions"/> by cloning the globally registered options
        /// and applying additional configuration.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to retrieve the registered <see cref="AspNetConventionOptions"/> instance.</param>
        /// <param name="configure">An optional delegate to configure <see cref="AspNetConventionOptions"/>.</param>
        /// <returns>A new <see cref="AspNetConventionOptions"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
        /// <remarks>
        /// This method creates a deep copy of the globally registered options to prevent mutation of the original instance.
        /// </remarks>
        internal static AspNetConventionOptions BuildAspNetConventionOptions(this IServiceProvider serviceProvider, Action<AspNetConventionOptions>? configure = null)
        {
            // Get the registered options
            var globalOptions = serviceProvider.GetRequiredService<IOptions<AspNetConventionOptions>>().Value;

            // Clone the options to avoid mutating the original instance
            var options = (AspNetConventionOptions)globalOptions.Clone();

            // Apply user configuration
            configure?.Invoke(options);

            return options;
        }

        /// <summary>
        /// Retrieves the globally registered <see cref="AspNetConventionOptions"/> from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider containing the options.</param>
        /// <returns>The registered <see cref="AspNetConventionOptions"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the options are not registered.</exception>
        /// <remarks>
        /// This method returns the original registered options instance. Any modifications will affect the global configuration.
        /// Use <see cref="BuildAspNetConventionOptions"/> if you need a mutable copy.
        /// </remarks>
        internal static AspNetConventionOptions GetAspNetConventionOptions(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IOptions<AspNetConventionOptions>>().Value;
        }

        /// <summary>
        /// Retrieves the registered options of the specified type from the service provider.
        /// </summary>
        /// <typeparam name="TOption">The type of options to retrieve.</typeparam>
        /// <param name="serviceProvider">The service provider containing the options.</param>
        /// <returns>An <see cref="IOptions{TOption}"/> wrapper for the specified options type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the specified options type is not registered.</exception>
        /// <remarks>
        /// This is a generic helper method that simplifies retrieving strongly-typed options from the DI container.
        /// </remarks>
        internal static IOptions<TOption> GetOptions<TOption>(this IServiceProvider serviceProvider) where TOption : class
        {
            return serviceProvider.GetRequiredService<IOptions<TOption>>();
        }
    }
}
