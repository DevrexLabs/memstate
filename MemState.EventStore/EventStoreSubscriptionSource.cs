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
            var ready = false;

            var sub = _connection.SubscribeToStreamFrom(
                _streamName, 
                from, 
                _settings, 
                (s, re) => handler.Invoke(re.Event.ToJournalRecord(_serializer)), s =>  ready = true );
            return new EventStoreSubscriptionAdapter(_connection,sub, () => ready);
        }
    }
}