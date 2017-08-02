using System;

namespace Memstate.Core
{
    public interface IJournalSubscriptionSource
    {
        IJournalSubscription Subscribe(long from, Action<JournalRecord> handler);
    }
}