using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.Trello.Core
{
    public class GetCardsQuery : Query<TrelloModel, List<Card>>
    {
        public string ColumnId { get; set; }

        public override List<Card> Execute(TrelloModel model)
        {
            return model.Boards.Values
                .SelectMany(b => b.Columns)
                .Single(c => c.Id == ColumnId)
                .Cards;
        }
    }
}