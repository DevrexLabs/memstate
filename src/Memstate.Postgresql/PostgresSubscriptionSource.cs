using System;

namespace Memstate.Postgres
{
    public class PostgresSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly PostgresSettings _settings;

        public PostgresSubscriptionSource(PostgresSettings settings)
        {
            Ensure.NotNull(settings, nameof(settings));
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