using System;

namespace Memstate.Postgresql
{
    public class PostgresqlSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly MemstateSettings _settings;

        public PostgresqlSubscriptionSource(MemstateSettings settings)
        {
            _settings = settings;
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new PostgresqlJournalSubscription(_settings, handler);
            
            subscription.Start();

            subscription.CatchUp(from);

            return subscription;
        }
    }
}