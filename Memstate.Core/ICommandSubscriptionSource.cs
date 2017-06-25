using System;

namespace Memstate.Core
{
    public interface ICommandSubscriptionSource
    {
        ICommandSubscription Subscribe(long from, Action<JournalRecord> handler);
    }
}