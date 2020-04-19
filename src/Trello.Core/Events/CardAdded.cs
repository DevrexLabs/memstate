namespace Memstate.Examples.Trello.Core
{
    public class CardAdded : Event
    {
        public string BoardId { get; set; }

        public string ColumnId { get; set; }
        public Card Card { get; set; }
    }
}