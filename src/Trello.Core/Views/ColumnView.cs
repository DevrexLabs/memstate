using System;

namespace Memstate.Examples.Trello.Core
{
    public class ColumnView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ColumnView(CardColumn column)
        {
            Id = column.Id;
            Name = column.Name;
        }
    }
}