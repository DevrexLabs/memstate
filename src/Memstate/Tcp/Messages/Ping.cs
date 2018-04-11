using System;

namespace Memstate.Tcp
{
    internal class Ping : Request
    {
        public Ping(Guid id)
        {
            Id = id;
        }

        public Ping() : this(Guid.NewGuid())
        {
        }
    }
}