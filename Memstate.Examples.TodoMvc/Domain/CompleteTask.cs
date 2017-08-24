using System;
using System.Linq;
using Newtonsoft.Json;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class CompleteTask : Command<TodoModel>
    {
        public CompleteTask()
        {
        }

        public CompleteTask(Guid listId, Guid taskId)
        {
            ListId = listId;
            TaskId = taskId;
        }
        
        [JsonProperty]
        public Guid ListId { get; private set; }
        
        [JsonProperty]
        public Guid TaskId { get; private set; }

        public override void Execute(TodoModel model)
        {
            var list = model.Lists[ListId];

            var task = list.Tasks.First(x => x.Id == TaskId);
            
            task.Complete();
        }
    }
}