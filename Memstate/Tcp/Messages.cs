using System;

namespace Memstate.Tcp
{
    internal class Response : NetworkMessage
    {
        public Response(Guid responseTo)
        {
            ResponseTo = responseTo;
        }

        public Guid ResponseTo { get; }
    }

    internal class QueryResponse : Response
    {
        public QueryResponse(object result, Guid responseTo)
            : base(responseTo)
        {
            Result = result;
        }

        public object Result { get; }
    }
}
