using System;

namespace Memstate.Core
{
    public interface ICommandChunkWriter : IDisposable
    {
        void Write(CommandChunk chunk);
    }
}