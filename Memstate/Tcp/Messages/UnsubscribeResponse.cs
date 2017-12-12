using System;

namespace Memstate.Tcp
{
    internal class UnsubscribeResponse : Response
    {
        public UnsubscribeResponse(Guid responseTo)
            : base(responseTo)
        {
        }
    }
}