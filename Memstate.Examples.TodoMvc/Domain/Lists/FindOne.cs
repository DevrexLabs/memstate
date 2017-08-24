using System;

namespace Memstate.Examples.TodoMvc.Domain.Lists
{
    public class FindOne : Query<TodoModel, TaskList>
    {
        public FindOne(Guid listId)
        {
            ListId = listId;
        }
        
        public Guid ListId { get; }

        public override TaskList Execute(TodoModel model)
        {
            var list = model.Lists[ListId];

            return list;
        }
    }
}