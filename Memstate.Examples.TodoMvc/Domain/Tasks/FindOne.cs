using System;
using System.Linq;
using Newtonsoft.Json;

namespace Memstate.Examples.TodoMvc.Domain.Tasks
{
    public class FindOne : Command<TodoModel, Task>
    {
        public FindOne(Guid listId, Guid taskId)
        {
            ListId = listId;
            TaskId = taskId;
        }

        [JsonProperty]
        public Guid ListId { get; private set; }

        [JsonProperty]
        public Guid TaskId { get; private set; }

        public override Task Execute(TodoModel model)
        {
            if (!model.Lists.TryGetValue(ListId, out var list))
            {
                return null;
            }

            var task = list.Tasks.FirstOrDefault();

            return task;
        }
    }
}