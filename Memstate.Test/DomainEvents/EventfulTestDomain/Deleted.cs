using System;

namespace Memstate.Test.EventfulTestDomain
{
    public class Deleted : Event
    {
        public Deleted(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; private set; }
    }
}