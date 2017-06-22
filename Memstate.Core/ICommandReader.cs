using System;
using System.Collections.Generic;

namespace Memstate.Core
{
    public interface ICommandReader : IDisposable
    {
        IEnumerable<Command> Read(ulong from);
    }
}