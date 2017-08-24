using System;
using System.Collections.Generic;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class Category
    {
        private readonly HashSet<Task> _tasks = new HashSet<Task>();
        
        public Category(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public Guid Id { get; }
        
        public string Name { get; }

        public IEnumerable<Task> Tasks => _tasks;

        public bool AddTask(Task task)
        {
            return _tasks.Add(task);
        }

        public bool RemoveTask(Task task)
        {
            return _tasks.Remove(task);
        }
    }
}