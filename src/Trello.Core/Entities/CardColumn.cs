using System;
using System.Collections.Generic;

namespace Memstate.Examples.Trello.Core
{
    public class CardColumn
    {
        public CardColumn(string id, string name)
        {
            Name = name;
            Id = id;
            Cards = new List<Card>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
    }
}