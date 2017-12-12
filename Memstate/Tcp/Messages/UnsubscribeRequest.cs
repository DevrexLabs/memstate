using System;
using Newtonsoft.Json;

namespace Memstate.Tcp
{
    internal class UnsubscribeRequest : Request
    {
        public UnsubscribeRequest(Type type)
        {
            Type = type;
        }

        [JsonProperty]
        public Type Type { get; private set; }
    }
}