using System;

namespace Memstate.Tcp
{
    internal class UnsubscribeRequest : Request
    {
        public UnsubscribeRequest(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }
    }
}