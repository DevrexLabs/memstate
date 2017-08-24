using System;

namespace Memstate
{
    public interface IJournalSubscriptionSource
    {
        IJournalSubscription Subscribe(long from, Action<JournalRecord> handler);
    }
}