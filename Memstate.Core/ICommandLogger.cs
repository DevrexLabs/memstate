using System;

namespace Memstate.Core
{
    public interface ICommandLogger : IDisposable
    {
        void Append(Command command);
    }
}