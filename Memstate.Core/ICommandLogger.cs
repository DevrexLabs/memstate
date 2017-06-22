using System;

namespace Memstate.Core
{
    public interface ICommandLogger
    {
        void AppendAsync(Command command);
    }
}