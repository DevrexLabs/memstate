using System;
using EventStore.ClientAPI;
using Memstate.Configuration;
using Memstate.Logging;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionSource
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;
        private readonly ILog _logger;
        private readonly EventStoreSettings _settings;

        public EventStoreSubscriptionSource(IEventStoreConnection connection)
        {
            _logger = LogProvider.GetCurrentClassLogger();
            _connection = connection;
            var config = Config.Current;
            _settings = config.GetSettings<EventStoreSettings>();
            _serializer = config.CreateSerializer(_settings.SerializerName);
            _streamName = _settings.StreamName;
        }

        public void Subscribe(long from, Action<JournalRecord> handler)
        {

            //return new EventStoreSubscriptionAdapter(_settings, sub, () => ready);
        }
    }
}