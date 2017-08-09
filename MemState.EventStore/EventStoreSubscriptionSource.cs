using System;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;
        private readonly CatchUpSubscriptionSettings _settings;

        public EventStoreSubscriptionSource(IEventStoreConnection connection, 
            ISerializer serializer, 
            string streamName,
            CatchUpSubscriptionSettings subscriptionSettings = null
        )
        {
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
                eventAppeared: (s, re) => handler.Invoke(re.Event.ToJournalRecord(_serializer)),
                liveProcessingStarted: s =>  ready = true );
            return new EventStoreSubscriptionAdapter(sub, () => ready);
        }
    }
}