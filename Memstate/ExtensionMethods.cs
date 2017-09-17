using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    internal static class ExtensionMethods
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

        public static async Task Wait(this TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(timeSpan, cancellationToken);
            }
            catch (TaskCanceledException)
            {

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
