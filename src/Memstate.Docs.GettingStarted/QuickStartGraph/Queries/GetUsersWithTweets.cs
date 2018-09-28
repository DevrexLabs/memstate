using Memstate.Models.Graph;
using System.Collections.Generic;
using System.Linq;
using static Memstate.Models.Graph.GraphModel;

namespace Memstate.Docs.GettingStarted.QuickStartGraph.Queries
{
    public class GetUsersWithTweets : Query<GraphModel, IEnumerable<Node>>
    {
        public override IEnumerable<Node> Execute(GraphModel db)
        {
            var users = db.Nodes.Where(n => n.Label == "user");
            var tweetEdges = users.SelectMany(e => e.Out.Where(r => r.Label == "tweeted"));
            var tweets = tweetEdges.Select(e => e.To);
            return tweets;
        }
    }
}
