using System;
using System.Collections.Generic;

namespace Memstate.Postgres.Tests.Domain
{
    public class Todo
    {
        public Dictionary<Guid, Task> Tasks { get; set; }
    }
}