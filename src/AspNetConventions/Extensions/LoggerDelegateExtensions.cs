using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides compiled logging extensions for efficient structured logging.
    /// </summary>
    /// <remarks>
    /// This class uses the .NET source generator for high-performance structured logging.
    /// The logging methods are compiled at build time, eliminating runtime string allocation
    /// and providing better performance compared to traditional string interpolation logging.
    /// </remarks>
    internal static partial class LoggerDelegateExtensions
    {
        private const string EventNamePrefix = "[AspNetConventions] :";
        private const string ExceptionDescriptorTemplate
            = "StatusCode={statusCode}, ErrorType={type}, Message={message}";

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = $"{EventNamePrefix} {ExceptionDescriptorTemplate}")]
        private static partial void LogExceptionDescriptorError(
            this ILogger logger, HttpStatusCode? statusCode, string? type, string? message, Exception? exception);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = $"{EventNamePrefix} {ExceptionDescriptorTemplate}")]
        private static partial void LogExceptionDescriptorWarning(
            this ILogger logger, HttpStatusCode? statusCode, string? type, string? message, Exception? exception);

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = $"{EventNamePrefix} {ExceptionDescriptorTemplate}")]
        private static partial void LogExceptionDescriptorInfo(
            this ILogger logger, HttpStatusCode? statusCode, string? type, string? message, Exception? exception);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = $"{EventNamePrefix} {ExceptionDescriptorTemplate}")]
        private static partial void LogExceptionDescriptorDebug(
            this ILogger logger, HttpStatusCode? statusCode, string? type, string? message, Exception? exception);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = $"{EventNamePrefix} {ExceptionDescriptorTemplate}")]
        private static partial void LogExceptionDescriptorTrace(
            this ILogger logger, HttpStatusCode? statusCode, string? type, string? message, Exception? exception);

        [LoggerMessage(
            Level = LogLevel.Critical,
            Message = $"{EventNamePrefix} {ExceptionDescriptorTemplate}")]
        private static partial void LogExceptionDescriptorCritical(
            this ILogger logger, HttpStatusCode? statusCode, string? type, string? message, Exception? exception);

        internal static void LogExceptionDescriptor(
            this ILogger logger,
            LogLevel level,
            HttpStatusCode? statusCode,
            string? type,
            string? message,
            Exception? exception)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    LogExceptionDescriptorCritical(logger, statusCode, type, message, exception);
                    break;
                case LogLevel.Error:
                    LogExceptionDescriptorError(logger, statusCode, type, message, exception);
                    break;
                case LogLevel.Warning:
                    LogExceptionDescriptorWarning(logger, statusCode, type, message, exception);
                    break;
                case LogLevel.Information:
                    LogExceptionDescriptorInfo(logger, statusCode, type, message, exception);
                    break;
                case LogLevel.Trace:
                    LogExceptionDescriptorTrace(logger, statusCode, type, message, exception);
                    break;
                default:
                    LogExceptionDescriptorDebug(logger, statusCode, type, message, exception);
                    break;
            }
        }

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = $"{EventNamePrefix} Action={{action}}, Type={{type}}, Name={{name}}, Message={{message}}")]
        public static partial void LogBindingMetadataDebug(
            this ILogger logger, string action, string type, string name, string? message = null);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = $"{EventNamePrefix} Name={{name}}, size={{size}}, Message=Cache size contains {{size}} items.")]
        private static partial void LogCacheSizeDebug(
            this ILogger logger, string name, int size);

        [LoggerMessage(
            Level = LogLevel.Information,
            Message = $"{EventNamePrefix} Name={{name}}, size={{size}}, Message=Cache size is growing large.")]
        private static partial void LogCacheSizeInfo(
            this ILogger logger, string name, int size);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = $"{EventNamePrefix} Name={{name}}, size={{size}}, Message=Cache size approaching critical levels.")]
        private static partial void LogCacheSizeWarning(
            this ILogger logger, string name, int size);

        /// <summary>
        /// Monitors and logs cache size information for diagnostic purposes.
        /// Logs at different levels based on the size thresholds to help identify potential memory issues without overwhelming the logs with low-level details.
        /// </summary>
        /// <param name="logger">The logger instance to use for logging cache size information.</param>
        /// <param name="name">The name of the cache being monitored, used for log context.</param>
        /// <param name="count">The current cache size count.</param>
        /// <param name="lastLoggedCount">The last logged count to prevent duplicate logging.</param>
        /// <returns>The current count if logging occurred; otherwise, null.</returns>
        internal static int? LogCacheSize(this ILogger logger, string name, int count, int lastLoggedCount)
        {
            if (count >= 100 && count != lastLoggedCount)
            {
                switch (count)
                {
                    case > 500:
                        LogCacheSizeWarning(logger, name, count);
                        break;
                    case > 200:
                        LogCacheSizeInfo(logger, name, count);
                        break;
                    default:
                        LogCacheSizeDebug(logger, name, count);
                        break;
                }

                return count;
            }

            return null;
        }

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = $"{EventNamePrefix} Message={{message}}")]
        public static partial void LogException(
            this ILogger logger, string message, Exception exception);

        [LoggerMessage(
            Level = LogLevel.Warning,
            Message = $"{EventNamePrefix} Sensitive Information disclosure vulnerability, Message={{message}}")]
        public static partial void LogDisclosureVulnerabilityWarning(
            this ILogger logger, string message);

    }
}
