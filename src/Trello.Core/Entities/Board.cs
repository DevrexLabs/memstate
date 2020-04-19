using System;
using System.Collections.Generic;

namespace Memstate.Examples.Trello.Core
{
    public class Board
    {
        public Board(string id, string name)
        {
            Name = name;
            Id = id;
            Columns = new List<CardColumn>();
        }

        public void AddColumn(string id, string name)
        {
            var column = new CardColumn(id, name);
            Columns.Add(column);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public List<CardColumn> Columns { get; set; }
    }
}