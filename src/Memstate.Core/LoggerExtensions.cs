using System;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    public static class LoggerExtensions
    {
        private static readonly EventId DefaultEventId = new EventId();

        public static void LogError(this ILogger logger, Exception ex, string message, params object[] args)
        {
            logger.LogError(DefaultEventId, ex, message, args);
        }
    }
}