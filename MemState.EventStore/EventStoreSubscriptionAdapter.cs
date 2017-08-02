using System;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionAdapter : IJournalSubscription
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