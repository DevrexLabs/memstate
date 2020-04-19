using System;
using System.Linq;

namespace Memstate.Examples.Trello.Core
{
    public class AddCardCommand : Command<TrelloModel, string>
    {
        public string BoardId { get; set; }
        public string ColumnId { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }

        public override string Execute(TrelloModel model)
        {
            var card = new Card(model.NextId(), Title)
            {
                Notes = Notes
            };

            model.Boards[BoardId]
                .Columns
                .Single(c => c.Id == ColumnId)
                .Cards
                .Add(card);

            RaiseEvent(new CardAdded
            {
                BoardId = BoardId,
                ColumnId = ColumnId,
                Card = card
            });

            return card.Id;
        }
    }
}