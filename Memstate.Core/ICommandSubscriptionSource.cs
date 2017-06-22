using System;

namespace Memstate.Core
{
    public interface ICommandSubscriptionSource
    {
        IDisposable Subscribe(ulong from, Action<Command> handler);
    }
}