using Memstate.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Memstate.Models.Graph.GraphModel;

namespace Memstate.Docs.GettingStarted.QuickStartGraph.Commands
{
    public class IncrementLikes : Command<GraphModel, Node>
    {
        public long TweetId { get; private set; }

        public IncrementLikes(long id)
        {
            TweetId = id;
        }

        public override Node Execute(GraphModel model)
        {
            var tweet = model.Nodes.SingleOrDefault(n => n.Id == TweetId);
            object likes = tweet.Get("likes") ?? (long)0;
            tweet.Set("likes", (long)likes + 1);
            return tweet;
        }
    }
}