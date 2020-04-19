using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.Trello.Core
{
    public class RenameColumnCommand : Command<TrelloModel>
    {
        public string ColumnId { get; set; }
        public string OldName { get; set; }
        public string NewName { get; set; }
        public override void Execute(TrelloModel model)
        {
            var column = model.Boards.Values
                .SelectMany(b => b.Columns)
                .SingleOrDefault(c => c.Id == ColumnId);

            if (column == null) throw new KeyNotFoundException("No such column");
            if (column.Name != OldName) throw new Exception("Current column name does not match OldName");
            column.Name = NewName;
        }
    }
}