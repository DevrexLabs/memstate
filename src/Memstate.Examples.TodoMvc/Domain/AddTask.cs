using System;
using Newtonsoft.Json;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class AddTask : Command<TodoModel, Task>
    {
        public AddTask()
        {
        }

        public AddTask(Guid listId, string title, string description, DateTime? dueBy)
        {
            ListId = listId;
            TaskId = Guid.NewGuid();
            Title = title;
            Description = description;
            DueBy = dueBy;
        }
        
        [JsonProperty]
        public Guid ListId { get; private set; }

        [JsonProperty]
        public Guid TaskId { get; private set; }
        
        [JsonProperty]
        public string Title { get; private set; }
        
        [JsonProperty]
        public string Description { get; private set; }
        
        [JsonProperty]
        public DateTime? DueBy { get; private set; }
        
        public override Task Execute(TodoModel model)
        {
            var task = new Task(TaskId, Title, Description, DueBy);

            var list = model.Lists[ListId];
            
            list.Add(task);

            return task;
        }
    }
}