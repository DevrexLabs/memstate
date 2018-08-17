using System;
using EventStore.ClientAPI;
using Memstate.Configuration;
using Memstate.Logging;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionSource : IJournalSubscriptionSource
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
            _settings = config.Resolve<EventStoreSettings>();
            _serializer = config.CreateSerializer(_settings.SerializerName);
            _streamName = _settings.StreamName;
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            long? checkPoint = null;

            if (from > 0)
            {
                checkPoint = from - 1;
            }

            var ready = false;

            var sub = _connection.SubscribeToStreamFrom(
                stream: _streamName,
                lastCheckpoint: checkPoint,
                settings: new CatchUpSubscriptionSettings(10000, 4096, false, false),
                eventAppeared: (s, re) =>
                {
                    _logger.Debug("eventAppeared, recordNumber {0}", re.OriginalEventNumber);
                    handler.Invoke(re.Event.ToJournalRecord(_serializer));
                },
                liveProcessingStarted: s =>
                {
                    ready = true;
                    _logger.Info("liveProcessingStarted");
                });
            return new EventStoreSubscriptionAdapter(_settings, sub, () => ready);
        }
    }
}