using System;
using Newtonsoft.Json;

namespace Memstate.Test.EventfulTestDomain
{
    public class Created : Event
    {
        private Created()
        {
        }

        public Created(Guid userId)
        {
            UserId = userId;
        }

        [JsonProperty]
        public Guid UserId { get; private set; }
    }
}