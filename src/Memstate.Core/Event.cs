using System;

namespace Memstate
{
    public abstract class Event
    {
        protected Event()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}