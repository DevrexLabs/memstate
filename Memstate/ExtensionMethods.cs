using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    public static class ExtensionMethods
    {
        public static T TakeOrDefault<T>(this BlockingCollection<T> collection, CancellationToken cancellationToken)
        {
            try
            {
                return collection.Take(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return default(T);
            }
        }
    }

    public static class LoggerExtensions
    {
        private static readonly EventId DefaultEventId = new EventId();

        public static void LogError(this ILogger logger, Exception ex, String message, params object[] args)
        {
            logger.LogError(DefaultEventId, ex, message, args);
        }
    }
}
