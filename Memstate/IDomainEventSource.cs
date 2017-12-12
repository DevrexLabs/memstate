using System;

namespace Memstate
{
    public interface IDomainEventSource
    {
        event Action<Event> EventRaised;
    }
}