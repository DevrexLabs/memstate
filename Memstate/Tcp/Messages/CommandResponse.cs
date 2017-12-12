using System;

namespace Memstate.Tcp
{
    internal class CommandResponse : Response
    {
        public CommandResponse(object result, Guid responseTo)
            : base(responseTo)
        {
            Result = result;
        }

        public object Result { get; }
    }
}