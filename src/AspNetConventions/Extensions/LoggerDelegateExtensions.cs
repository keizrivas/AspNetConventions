using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Extensions
{
    internal static partial class LoggerDelegateExtensions
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "Processing item {ItemId} of type {ItemType}")]
        public static partial void LogException(
            this ILogger logger, int itemId, string itemType);
    }
}
