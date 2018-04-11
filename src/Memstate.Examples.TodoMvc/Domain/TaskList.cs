using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class TaskList
    {
        private readonly HashSet<Task> _tasks = new HashSet<Task>();
        
        public TaskList(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public Guid Id { get; }

        public string Name { get; private set; }

        public IEnumerable<Task> Tasks => _tasks.ToList();

        public void Rename(string name)
        {
            Name = name;
        }

        public void Add(Task task)
        {
            _tasks.Add(task);
        }

        public void Remove(Task task)
        {
            _tasks.Remove(task);
        }
    }
}