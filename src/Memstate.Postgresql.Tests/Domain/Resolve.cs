using System;
using Newtonsoft.Json;

namespace Memstate.Postgresql.Tests.Domain
{
    public class Resolve : Command<Todo>
    {
        public Resolve(Guid taskId)
        {
            TaskId = taskId;
        }
        
        [JsonProperty]
        public Guid TaskId { get; private set; }
        
        public override void Execute(Todo model)
        {
            model.Tasks.Remove(TaskId);
        }
    }
}