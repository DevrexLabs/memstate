using System;

namespace Memstate.Postgresql
{
    public class PostgresqlSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly PostgresqlSettings _settings;

        public PostgresqlSubscriptionSource(PostgresqlSettings settings)
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