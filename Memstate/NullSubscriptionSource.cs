using System;

namespace Memstate
{
    public class NullSubscriptionSource : IJournalSubscriptionSource
    {
        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            return new NullJournalSubscription();
        }
        
        public class NullJournalSubscription : IJournalSubscription
        {
            public void Dispose()
            {
            }

            public bool Ready()
            {
                return true;
            }
        }
    }
}