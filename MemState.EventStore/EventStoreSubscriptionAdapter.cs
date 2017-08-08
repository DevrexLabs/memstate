using System;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionAdapter : IJournalSubscription
        {
            private readonly EventStoreCatchUpSubscription _subscription;
            private readonly Func<bool> _ready;

            public bool Ready() => _ready.Invoke();
            
            public EventStoreSubscriptionAdapter(EventStoreCatchUpSubscription subscription, Func<bool> ready)
            {
                _subscription = subscription;
                _ready = ready;
            }

            public void Dispose()
            {
                _subscription.Stop();
            }
        }
    }