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
        private readonly ILogger _logger;

        public bool Ready() => _ready.Invoke();
            
        public EventStoreSubscriptionAdapter(Config config, EventStoreCatchUpSubscription subscription, Func<bool> ready)
        {
            _logger = config.CreateLogger<EventStoreSubscriptionAdapter>();
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