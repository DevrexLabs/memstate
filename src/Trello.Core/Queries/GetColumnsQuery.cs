using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.Trello.Core
{
    public class GetColumnsQuery : Query<TrelloModel, List<ColumnView>>
    {
        public GetColumnsQuery(string id)
        {
            BoardId = id;
        }

        public string BoardId { get; set; }

        public override List<ColumnView> Execute(TrelloModel model)
        {
            return model.Boards[BoardId].Columns
                .Select(c => new ColumnView(c)).ToList();
        }
    }
}