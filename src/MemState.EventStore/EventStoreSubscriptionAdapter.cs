using System;
using EventStore.ClientAPI;
using Memstate.Logging;


namespace Memstate.EventStore
{
    public class EventStoreSubscriptionAdapter : IJournalSubscription
    {
        private readonly EventStoreCatchUpSubscription _subscription;

        private readonly Func<bool> _ready;

        private readonly ILog _logger;

        public EventStoreSubscriptionAdapter(MemstateSettings config, EventStoreCatchUpSubscription subscription, Func<bool> ready)
        {
            _logger = LogProvider.GetCurrentClassLogger();
            _subscription = subscription;
            _ready = ready;
        }

        public bool Ready() => _ready.Invoke();

        public void Dispose()
        {
            _logger.Info("Disposing");
            _subscription.Stop();
        }
    }
}