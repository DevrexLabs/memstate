namespace Memstate.Examples.Trello.Core
{
    public class ColumnAdded : Event
    {
        public string BoardId { get; set; }
        public ColumnView Column { get; set; }
    }
}