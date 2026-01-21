using System;
using AspNetConventions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Extensions
{
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        /// Builds a new instance of <see cref="AspNetConventionOptions"/> by cloning the globally registered options
        /// and applying additional configuration.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to retrieve the registered <see cref="AspNetConventionOptions"/>instance.</param>
        /// <param name="configure">An optional delegate to configure <see cref="AspNetConventionOptions"/>.</param>
        /// <returns>A new <see cref="AspNetConventionOptions"/> instance.</returns>
        internal static AspNetConventionOptions BuildAspNetConventionOptions(this IServiceProvider serviceProvider, Action<AspNetConventionOptions>? configure = null)
        {
            // Get the registered options
            var globalOptions = serviceProvider.GetRequiredService<IOptions<AspNetConventionOptions>>().Value;
            var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<AspNetConventionOptions>>();

            // Clone the options to avoid mutating the original instance
            var options = (AspNetConventionOptions)globalOptions.Clone();

            // Apply user configuration
            configure?.Invoke(options);

            return options;
        }

        internal static AspNetConventionOptions GetAspNetConventionOptions(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IOptions<AspNetConventionOptions>>().Value;
        }

        internal static IOptions<TOption> GetOptions<TOption>(this IServiceProvider serviceProvider) where TOption : class
        {
            return serviceProvider.GetRequiredService<IOptions<TOption>>();
        }
    }
}
