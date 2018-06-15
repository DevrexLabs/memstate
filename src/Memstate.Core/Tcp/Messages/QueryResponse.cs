using System;

namespace Memstate.Tcp
{
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