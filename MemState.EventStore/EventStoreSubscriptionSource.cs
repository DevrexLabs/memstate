using System;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly Config _config;
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;
        private readonly CatchUpSubscriptionSettings _settings;

        private readonly ILogger _logger;

        public EventStoreSubscriptionSource(Config config, IEventStoreConnection connection, 
            ISerializer serializer, 
            string streamName,
            CatchUpSubscriptionSettings subscriptionSettings = null
        )
        {
            _logger = config.CreateLogger<EventStoreSubscriptionSource>();
            _config = config;
            _connection = connection;
            _serializer = serializer;
            _streamName = streamName;
            _settings = subscriptionSettings 
                        ?? new CatchUpSubscriptionSettings(10000, 4096, false, false);
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            long? checkPoint = null;
            if (from > 0) checkPoint = from - 1;

            var ready = false;
            var sub = _connection.SubscribeToStreamFrom(
                stream: _streamName, 
                lastCheckpoint: checkPoint, 
                settings: _settings, 
                eventAppeared: (s, re) =>
                {
                    _logger.LogDebug("eventAppeared, recordNumber {0}", re.OriginalEventNumber);
                    handler.Invoke(re.Event.ToJournalRecord(_serializer));
                },
                liveProcessingStarted: s =>
                {
                    ready = true;
                    _logger.LogInformation("liveProcessingStarted");
                });
            return new EventStoreSubscriptionAdapter(_config, sub, () => ready);
        }
    }
}