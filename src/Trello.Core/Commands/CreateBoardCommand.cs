namespace Memstate.Examples.Trello.Core
{
    public class CreateBoardCommand : Command<TrelloModel, string>
    {
        public string BoardName { get; set; }

        public CreateBoardCommand(string boardName)
        {
            BoardName = boardName;
        }

        public override string Execute(TrelloModel model)
            => model.CreateBoard(BoardName);
    }
}