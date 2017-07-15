using System;

namespace Memstate.Core
{
    public interface IJournalSubscriptionSource
    {
        ICommandSubscription Subscribe(long from, Action<JournalRecord> handler);
    }
}