using System;

namespace Memstate.Core
{
    public interface IJournalSubscription : IDisposable
    {
        bool Ready();
    }
}