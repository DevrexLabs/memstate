using System;
using Newtonsoft.Json;

namespace Memstate.Tcp
{
    internal class SubscribeRequest : Request
    {
        public SubscribeRequest(Type type, IEventFilter[] filters)
        {
            Type = type;
            Filters = filters;
        }

        [JsonProperty]
        public Type Type { get; private set; }

        [JsonProperty]
        public IEventFilter[] Filters { get; private set; }
    }
}