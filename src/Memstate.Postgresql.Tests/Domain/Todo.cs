using System;
using System.Collections.Generic;

namespace Memstate.Postgresql.Tests.Domain
{
    public class Todo
    {
        public Dictionary<Guid, Task> Tasks { get; set; }
    }
}