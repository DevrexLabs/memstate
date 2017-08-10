using System;
using EventStore.ClientAPI;
using Memstate.Core;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionAdapter : IJournalSubscription
        {
        private readonly EventStoreCatchUpSubscription _subscription;
        private readonly Func<bool> _ready;
        private readonly ILogger _logger = Logging.CreateLogger<EventStoreWriter>();

        public bool Ready() => _ready.Invoke();
            
            public EventStoreSubscriptionAdapter(EventStoreCatchUpSubscription subscription, Func<bool> ready)
            {
                _subscription = subscription;
                _ready = ready;
            }

            public void Dispose()
            {
                _logger.LogInformation("Disposing");
                _subscription.Stop();
            }
        }
    }