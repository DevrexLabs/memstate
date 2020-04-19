using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.Trello.Core
{
    public class GetBoardsQuery : Query<TrelloModel, List<BoardView>>
    {
        public override List<BoardView> Execute(TrelloModel model)
        {
            return model
                .Boards
                .Values
                .OrderBy(board => board.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(board => new BoardView(board))
                .ToList();
        }
    }
}