namespace Memstate.Models.Graph
{
    public partial class GraphModel
    {
        public class Edge : Item
        {
            public Edge(long id, string label) : base(id, label) { }

            public Node From;
            public Node To;
        }
    }
}
