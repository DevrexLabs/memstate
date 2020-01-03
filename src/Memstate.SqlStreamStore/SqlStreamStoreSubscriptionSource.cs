using System;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreSubscriptionSource : IJournalSubscriptionSource
    {
        public IJournalSubscription Subscribe(long @from, Action<JournalRecord> handler)
        {
            throw new NotImplementedException();
        }
    }
}