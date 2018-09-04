using System;
using Memstate.Test.EventfulTestDomain;

namespace Memstate.Test
{
    public partial class LocalClientEventTests
    {
        private class UserDeletedEventFilter : IEventFilter
        {
            public Guid Id { get; private set; }

            public UserDeletedEventFilter(Guid id)
            {
                Id = id;
            }

            public bool Accept(Event item)
            {
                return (item as Deleted)?.UserId == Id;
            }
        }
    }
}