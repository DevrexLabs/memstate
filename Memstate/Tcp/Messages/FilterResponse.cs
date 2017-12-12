using System;

namespace Memstate.Tcp
{
    internal class FilterResponse : Response
    {
        public FilterResponse(Guid responseTo)
            : base(responseTo)
        {
        }
    }
}