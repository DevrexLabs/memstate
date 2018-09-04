using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.TodoMvc.Domain.Lists
{
    public class FindAll : Query<TodoModel, IEnumerable<TaskList>>
    {
        public override IEnumerable<TaskList> Execute(TodoModel db)
        {
            var lists = db.Lists.Values.ToList();

            return lists;
        }
    }
}