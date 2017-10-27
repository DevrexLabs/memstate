using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreConnectionMonitor
    {
        public EventStoreConnectionMonitor(Settings config, IEventStoreConnection connection)
        {
            ILogger logger = config.CreateLogger<EventStoreConnectionMonitor>();
            string connectionName = connection.ConnectionName;
            connection.Closed        += (s, e) => logger.LogInformation("ES connection {0} closed, reason: {1}", connectionName, e.Reason);
            connection.Disconnected  += (s, e) => logger.LogWarning("ES disconnected, {0}", connectionName);
            connection.ErrorOccurred += (s, e) => logger.LogError("ES connection {0} error: ", default(EventId), e.Exception, connectionName);
            connection.Reconnecting  += (s, e) => logger.LogInformation("ES {0} reconnecting", connectionName);
            connection.Connected     += (s, e) => logger.LogInformation("ES {0} connected", connectionName);
        }
    }
}