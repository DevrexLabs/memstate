namespace Memstate.Examples.Trello.Core
{
    public class AddColumnCommand : Command<TrelloModel, string>
    {
        public string BoardId { get; set; }

        public string ColumnName { get; set; }

        public AddColumnCommand(string boardId, string columnName)
        {
            BoardId = boardId;
            ColumnName = columnName;
        }
        public override string Execute(TrelloModel model)
        {
            var columnId = model.NextId();
            var column = new CardColumn(columnId, ColumnName);

            model.Boards[BoardId].Columns.Add(column);

            RaiseEvent(new ColumnAdded
            {
                BoardId = BoardId,
                Column = new ColumnView(column)
            });

            return columnId;
        }
    }
}