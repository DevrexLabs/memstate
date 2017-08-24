using System;

namespace Memstate
{
    public interface IJournalSubscription : IDisposable
    {
        bool Ready();
    }
}