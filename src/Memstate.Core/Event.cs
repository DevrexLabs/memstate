using System;
using Newtonsoft.Json;

namespace Memstate
{
    public abstract class Event
    {
        protected Event()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
    }
}