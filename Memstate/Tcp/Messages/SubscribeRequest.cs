using System;

namespace Memstate.Tcp
{
    internal class SubscribeRequest : Request
    {
        public SubscribeRequest(Type type, IEventFilter filter)
        {
            Type = type;
            Filter = filter;
        }

        public Type Type { get; private set; }

        public IEventFilter Filter { get; private set; }
    }
}