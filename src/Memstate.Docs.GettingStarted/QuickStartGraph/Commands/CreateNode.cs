using Memstate.Models.Graph;
using System.Linq;
using static Memstate.Models.Graph.GraphModel;

namespace Memstate.Docs.GettingStarted.QuickStartGraph.Commands
{
    public class CreateNode : Command<GraphModel, Node>
    {
        public string Label { get; private set; }

        public CreateNode(string label)
        {
            Label = label;
        }

        public override Node Execute(GraphModel model)
        {
            var nodeId = model.CreateNode(Label);
            return model.Nodes.Single(n => n.Id == nodeId);
        }
    }
}
