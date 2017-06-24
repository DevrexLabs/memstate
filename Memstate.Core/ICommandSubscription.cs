using System;

namespace Memstate.Core
{
    public interface ICommandSubscription : IDisposable
    {
        bool Ready();
    }
}