using System;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly MemstateSettings _memstateSettings;

        private readonly IEventStoreConnection _connection;

        private readonly ISerializer _serializer;

        private readonly string _streamName;

        private readonly ILogger _logger;

        public EventStoreSubscriptionSource(MemstateSettings settings, IEventStoreConnection connection)
        {
            _logger = settings.CreateLogger<EventStoreSubscriptionSource>();
            _memstateSettings = settings;
            _connection = connection;

            var eventStoreSettings = new EventStoreSettings(settings);

            _serializer = eventStoreSettings.CreateSerializer();
            _streamName = eventStoreSettings.StreamName;
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
                    _logger.LogDebug("eventAppeared, recordNumber {0}", re.OriginalEventNumber);
                    handler.Invoke(re.Event.ToJournalRecord(_serializer));
                },
                liveProcessingStarted: s =>
                {
                    ready = true;
                    _logger.LogInformation("liveProcessingStarted");
                });
            return new EventStoreSubscriptionAdapter(_memstateSettings, sub, () => ready);
        }
    }
}