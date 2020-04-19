using System;

namespace Memstate.Examples.Trello.Core
{
    public class Card
    {
        public Card(string id, string title)
        {
            Title = title;
            Id = id;
        }
 
        public string Id { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
    }
}