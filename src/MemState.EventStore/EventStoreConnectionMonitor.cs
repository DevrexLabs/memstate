using EventStore.ClientAPI;
using Memstate.Logging;

namespace Memstate.EventStore
{
    public class EventStoreConnectionMonitor
    {
        public EventStoreConnectionMonitor(EngineSettings config, IEventStoreConnection connection)
        {
            ILog logger = LogProvider.GetCurrentClassLogger();
            
            var connectionName = connection.ConnectionName;
            
            connection.Closed        += (s, e) => logger.Info("ES connection {0} closed, reason: {1}", connectionName, e.Reason);
            connection.Disconnected  += (s, e) => logger.Warn("ES disconnected, {0}", connectionName);
            connection.ErrorOccurred += (s, e) => logger.Error(e.Exception, "ES connection {0} error: ", connectionName);
            connection.Reconnecting  += (s, e) => logger.Info("ES {0} reconnecting", connectionName);
            connection.Connected     += (s, e) => logger.Info("ES {0} connected", connectionName);
        }
    }
}