using System;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Core.Abstractions.Models
{
    /// <summary>
    /// Provides a base class for building HTTP responses using ASP.NET convention options.
    /// </summary>
    /// <param name="options">The ASP.NET convention options to use when building responses.</param>
    /// <param name="logger">Logger instance</param>
    internal abstract class ResponseAdapter(AspNetConventionOptions options, ILogger logger) : IResponseAdapter
    {
        /// <summary>
        /// Gets the logger instance for diagnostic information.
        /// </summary>
        /// <value>The logger instance provided during construction.</value>
        public ILogger Logger => logger;

        /// <summary>
        /// Gets the AspNetConventions configuration options.
        /// </summary>
        /// <value>The options instance provided during construction.</value>
        /// <exception cref="ArgumentNullException">Thrown when the options are null.</exception>
        public AspNetConventionOptions Options => options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Determines whether the specified data object represents a wrapped response.
        /// </summary>
        /// <param name="data">The data object to evaluate.</param>
        /// <returns>true if the data object is recognized as a wrapped response; otherwise, false.</returns>
        public abstract bool IsWrappedResponse(object? data);

        /// <summary>
        /// Monitors and logs cache size information for response factory caching.
        /// </summary>
        /// <param name="count">The current cache size count.</param>
        /// <param name="lastLoggedCount">The last logged count to prevent duplicate logging.</param>
        /// <returns>The current count if logging occurred; otherwise, null.</returns>
        internal int? MonitorCacheSize(int count, int lastLoggedCount)
        {
            if (count >= 100 && count != lastLoggedCount)
            {
                var logLevel = count switch
                {
                    > 500 => LogLevel.Warning,
                    > 200 => LogLevel.Information,
                    > 100 => LogLevel.Debug,
                    _ => LogLevel.Trace
                };

                if (count > 1000)
                {
                    Logger.LogWarning("Response factory cache is growing large: {Count} types", count);
                }
                else
                {
                    Logger.Log(logLevel, "Response factory cache size: {Count} types", count);
                }

                return count;
            }

            return null;
        }
    }
}
