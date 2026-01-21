using Microsoft.Extensions.Logging;

namespace AspNetConventions.Extensions
{
    internal static partial class LoggerDelegateExtensions
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "Processing item {ItemId} of type {ItemType}")]
        internal static partial void LogException(
            this ILogger logger, int itemId, string itemType);
    }
}
