using System;
using System.Collections.Generic;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class TodoModel
    {
        public IDictionary<Guid, TaskList> Lists { get; } = new Dictionary<Guid, TaskList>();
        public IDictionary<Guid, Category> Categories { get; } = new Dictionary<Guid, Category>();
    }
}