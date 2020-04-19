using System;
using System.Collections.Generic;

namespace Memstate.Examples.Trello.Core
{
    public class GetBoardQuery : Query<TrelloModel, BoardView>
    {
        public GetBoardQuery(string id)
        {
            BoardId = id;
        }
        public string BoardId { get; set; }

        public override BoardView Execute(TrelloModel model)
        {
            if (!model.Boards.TryGetValue(BoardId, out var board))
            {
                throw new KeyNotFoundException("No board with id " + BoardId);
            }
            return new BoardView(board);
        }
    }
}