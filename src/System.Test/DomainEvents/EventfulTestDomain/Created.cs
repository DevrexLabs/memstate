using System;

namespace Memstate.Test.EventfulTestDomain
{
    public class Created : Event
    {
        public Created(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; private set; }
    }
}