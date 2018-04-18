using System;

namespace Memstate.Tcp
{
    internal class SubscribeResponse : Response
    {
        public SubscribeResponse(Guid responseTo)
            : base(responseTo)
        {
        }
    }
}