using System;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionSource : ICommandSubscriptionSource
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
                        ?? new CatchUpSubscriptionSettings(10000, 10000, false, false);
        }

        public ICommandSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var ready = false;

            var sub = _connection.SubscribeToStreamFrom(
                _streamName, 
                from, 
                _settings, 
                (s, re) => handler.Invoke(JournalRecordFromEvent(re.Event)), s =>  ready = true );
            return new EventStoreSubscriptionAdapter(_connection,sub, () => ready);
        }

        private JournalRecord JournalRecordFromEvent(RecordedEvent @event)
        {
            var command = (Command) _serializer.Deserialize(@event.Data);
            return new JournalRecord(@event.EventNumber, @event.Created, command);
        }

        class EventStoreSubscriptionAdapter : ICommandSubscription
        {
            private readonly IEventStoreConnection _connection;
            private readonly EventStoreCatchUpSubscription _subscription;
            private readonly Func<bool> _ready;

            public bool Ready() => _ready.Invoke();
            
            public EventStoreSubscriptionAdapter(IEventStoreConnection connection, EventStoreCatchUpSubscription subscription, Func<bool> ready)
            {
                _connection = connection;
                _subscription = subscription;
                _ready = ready;
            }

            public void Dispose()
            {
                //todo: we don't own the connection, closing it here feels wrong
                _subscription.Stop();
                _connection.Close();
            }
        }
    }
}