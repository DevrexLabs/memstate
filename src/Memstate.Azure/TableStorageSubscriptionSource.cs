using System;

namespace Memstate.Azure
{
    public class TableStorageSubscriptionSource : IJournalSubscriptionSource
    {
        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            throw new NotImplementedException();
        }
    }
}