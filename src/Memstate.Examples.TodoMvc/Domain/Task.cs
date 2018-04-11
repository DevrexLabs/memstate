using System;
using System.Collections.Generic;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class Task : IComparable<Task>
    {
        private readonly HashSet<Category> _categories = new HashSet<Category>();
        
        public Task(Guid id, string title, string description, DateTime? dueBy = null)
        {
            Id = id;
            Title = title;
            Description = description;
            DueBy = dueBy;
        }

        public Guid Id { get; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<Category> Categories => _categories;

        public DateTime? DueBy { get; private set; }

        public DateTime? CompletedOn { get; private set; }

        public void Rename(string title)
        {
            Title = title;
        }

        public void Describe(string description)
        {
            Description = description;
        }

        public void Postpone(DateTime dueBy)
        {
            DueBy = dueBy;
        }

        public void Complete()
        {
            if (CompletedOn == null)
            {
                CompletedOn = DateTime.UtcNow;
            }
        }

        public int CompareTo(Task other)
        {
            return Id.CompareTo(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object other)
        {
            var task = other as Task;

            return task != null && task.Id == Id;
        }
    }
}