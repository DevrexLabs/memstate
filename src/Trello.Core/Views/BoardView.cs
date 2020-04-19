using System;

namespace Memstate.Examples.Trello.Core
{
    public class BoardView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public BoardView(Board board)
        {
            Id = board.Id;
            Name = board.Name;
        }
    }
}