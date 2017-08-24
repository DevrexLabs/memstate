using System;
using Newtonsoft.Json;

namespace Memstate.Examples.TodoMvc.Domain
{
    public class RemoveList : Command<TodoModel>
    {
        public RemoveList()
        {
        }

        public RemoveList(Guid listId)
        {
            ListId = listId;
        }

        [JsonProperty]
        public Guid ListId { get; private set; }
        
        public override void Execute(TodoModel model)
        {
            model.Lists.Remove(ListId);
        }
    }
}