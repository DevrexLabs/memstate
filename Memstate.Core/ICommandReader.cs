using System.Collections.Generic;

namespace Memstate.Core
{
    public interface ICommandReader
    {
        IEnumerable<Command> Read(ulong from);
    }
}