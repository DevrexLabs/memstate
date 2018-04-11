using System;

namespace Memstate.Postgresql.Tests.Domain
{
    public class Task
    {
        public Task(Guid id, string description)
        {
            Id = id;
            Description = description;
        }
        
        public Guid Id { get; }
        
        public string Description { get; }
    }
}