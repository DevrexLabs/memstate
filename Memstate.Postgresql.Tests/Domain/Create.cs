using System;
using Newtonsoft.Json;

namespace Memstate.Postgresql.Tests.Domain
{
    public class Create : Command<Todo, Task>
    {
        public Create(Guid taskId, string description)
        {
            TaskId = taskId;
            Description = description;
        }
        
        [JsonProperty]
        public string Description { get; private set; }
        
        [JsonProperty]
        public Guid TaskId { get; private set; }
        
        public override Task Execute(Todo model)
        {
            var task = new Task(TaskId, Description);
            
            model.Tasks.Add(TaskId, task);

            return task;
        }
    }
}