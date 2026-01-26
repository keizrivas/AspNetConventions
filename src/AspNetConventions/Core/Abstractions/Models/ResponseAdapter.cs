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
        public ILogger Logger => logger;

        public AspNetConventionOptions Options => options ?? throw new ArgumentNullException(nameof(options));

        public abstract bool IsWrappedResponse(object? data);

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
