using Memstate.Models.Graph;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Memstate.Test.Models
{
    public class GraphModelTests
    {
        private GraphModel _graph;
        private long _user1;

        [SetUp]
        public void Init()
        {
            _graph = new GraphModel();
            _user1 = _graph.CreateNode("user");
            var user2 = _graph.CreateNode("user");
            var tweet = _graph.CreateNode("tweet");
            _graph.CreateEdge(_user1, tweet, "tweeted");
            _graph.CreateEdge(user2, tweet, "retweeted");
            _graph.CreateEdge(_user1, user2, "followed");
            _graph.CreateEdge(_user1, _user1, "followed");
        }

        [Test]
        public void Test_how_many_users_have_one_tweet()
        {
            Expression<Func<GraphModel, int>> query =
                g => g.Nodes.Count(n => n.Label == "user" && n.Out.Any(e => e.Label == "tweeted"));
            var result = _graph.Query(query);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Test_what_is_the_maximum_number_of_retweets_for_any_tweet()
        {
            Expression<Func<GraphModel, int>> query =
                g => g.Nodes.Where(n => n.Label == "tweet")
                .Max(n => n.In.Count(e => e.Label == "retweeted"));
            var result = _graph.Query(query);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Test_nodes_having_self_references()
        {
            Expression<Func<GraphModel, IEnumerable<GraphModel.Node>>> query =
                g => g.Nodes.Where(n => n.Out.Any(e => n.In.Contains(e)));

            var node = _graph.Query(query).Single();
            Assert.AreEqual(node.Id, _user1);
        }

        [Test]
        public void Test_setting_and_getting_props()
        {
            Expression<Func<GraphModel, GraphModel.Node>> query =
                g => g.Nodes.First(n => n.Label == "tweet");
            var node = _graph.Query(query);
            Assert.AreEqual(typeof(GraphModel.Node), node.GetType());

            node.Set("testkey", "testvalue");

            node = _graph.Query(query);
            Assert.AreEqual("testvalue", node.Props["testkey"]);
        }
    }
}
