using System;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreSubscriptionAdapter : IJournalSubscription
    {
        private readonly EventStoreCatchUpSubscription _subscription;

        private readonly Func<bool> _ready;

        private readonly ILogger _logger;

        public EventStoreSubscriptionAdapter(MemstateSettings config, EventStoreCatchUpSubscription subscription, Func<bool> ready)
        {
            _logger = config.CreateLogger<EventStoreSubscriptionAdapter>();
            _subscription = subscription;
            _ready = ready;
        }

        public bool Ready() => _ready.Invoke();

        public void Dispose()
        {
            _logger.LogInformation("Disposing");

            _subscription.Stop();
        }
    }
}