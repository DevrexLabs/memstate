using System;

namespace Memstate.Tcp
{
    internal class Response : Message
    {
        public Response(Guid responseTo)
        {
            ResponseTo = responseTo;
        }

        public Guid ResponseTo { get; }
    }
}