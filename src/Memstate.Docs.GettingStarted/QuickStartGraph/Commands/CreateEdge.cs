using Memstate.Models.Graph;
using System.Linq;
using static Memstate.Models.Graph.GraphModel;

namespace Memstate.Docs.GettingStarted.QuickStartGraph.Commands
{
    public class CreateEdge : Command<GraphModel, Edge>
    {
        public long From { get; private set; }
        public long To { get; private set; }
        public string Label { get; private set; }

        public CreateEdge(long from, long to, string label)
        {
            From = from;
            To = to;
            Label = label;
        }

        public override Edge Execute(GraphModel model)
        {
            var edgeId = model.CreateEdge(From, To, Label);
            return model.Edges.Single(e => e.Id == edgeId);
        }
    }
}
