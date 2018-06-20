using System;

namespace Memstate.Postgresql
{
    public class PostgresSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly MemstateSettings _settings;

        public PostgresSubscriptionSource(MemstateSettings settings)
        {
            _settings = settings;
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new PostgresJournalSubscription(_settings, handler, from);
            
            subscription.Start();

            return subscription;
        }
    }
}