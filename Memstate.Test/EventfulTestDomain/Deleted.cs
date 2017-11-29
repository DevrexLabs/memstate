using System;
using Newtonsoft.Json;

namespace Memstate.Test.EventfulTestDomain
{
    public class Deleted : Event
    {
        private Deleted()
        {
        }

        public Deleted(Guid userId)
        {
            UserId = userId;
        }

        [JsonProperty]
        public Guid UserId { get; private set; }
    }
}