using System;
using Newtonsoft.Json;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class CreateList : Command<TodoModel, TaskList>
    {
        public CreateList()
        {
        }
        
        public CreateList(string name)
        {
            ListId = Guid.NewGuid();
            Name = name;
        }
        
        [JsonProperty]
        public Guid ListId { get; private set; }
        
        [JsonProperty]
        public string Name { get; private set; }
        
        public override TaskList Execute(TodoModel model)
        {
            var list = new TaskList(ListId, Name);
            
            model.Lists.Add(list.Id, list);

            return list;
        }
    }
}