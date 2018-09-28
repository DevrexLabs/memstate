using System.Collections.Generic;

namespace Memstate.Models.Graph
{
    public partial class GraphModel
    {
        public class Node : Item
        {
            public Node(long id, string label) : base(id, label) { }

            public ISet<Edge> Out = new SortedSet<Edge>();
            public ISet<Edge> In = new SortedSet<Edge>();
        }
    }
}
