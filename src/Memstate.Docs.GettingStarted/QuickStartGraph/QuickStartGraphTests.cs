using Memstate.Configuration;
using Memstate.Docs.GettingStarted.QuickStartGraph.Commands;
using Memstate.Docs.GettingStarted.QuickStartGraph.Queries;
using Memstate.Models.Graph;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate.Docs.GettingStarted.QuickStartGraph
{
    public class QuickStartGraphTests
    {
        private string JournalFile = "smoke_test_with_defaults_wire_for_graph";
        private string JournalFilename = "smoke_test_with_defaults_wire_for_graph.journal";
        private string JournalSerializer = "Wire";

        [SetUp]
        [TearDown]
        public void SetupTeardown()
        {
            if (File.Exists(JournalFilename)) File.Delete(JournalFilename);
            Config.Reset();
        }

        [Test]
        public async Task Simple_end_to_end_getting_started_using_graphmodel_and_default_wire_serializer_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a graph model using default settings");
            Print("   (using Wire format & local filesystem storage)");
            var config = Config.Current;
            config.SerializerName = JournalSerializer;
            var settings = config.GetSettings<EngineSettings>();
            settings.StreamName = JournalFile;
            var engine = await Engine.Start<GraphModel>();

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists(JournalFilename));

            Print("WHEN I add some graph data");
            var _user1 = await engine.Execute(new CreateNode("user"));
            var _user2 = await engine.Execute(new CreateNode("user"));
            var _tweet = await engine.Execute(new CreateNode("tweet"));

            await engine.Execute(new CreateEdge(_user1.Id, _tweet.Id, "tweeted"));
            await engine.Execute(new CreateEdge(_user2.Id, _tweet.Id, "retweeted"));
            await engine.Execute(new CreateEdge(_user1.Id, _user2.Id, "followed"));
            await engine.Execute(new CreateEdge(_user1.Id, _user1.Id, "followed"));

            Print("THEN there should be three nodes");
            var nodes = await engine.Execute(new GetNodes());
            Assert.AreEqual(3, nodes.Count());

            Print("THEN there should be four edges");
            var edges = await engine.Execute(new GetEdges());
            Assert.AreEqual(4, edges.Count());

            Print("THEN there should be one user with one tweet");
            var usersWithTweets = await engine.Execute(new GetUsersWithTweets());
            Assert.AreEqual(1, usersWithTweets.Count());

            Print("THEN there should be one tweet with null likes");
            Assert.AreEqual(null, usersWithTweets.First().Get("likes"));

            Print("WHEN I increment likes");
            var tweet = await engine.Execute(new IncrementLikes(_tweet.Id));

            Print("THEN there should be one like");
            Assert.AreEqual(1, tweet.Get("likes"));

            Print("WHEN I dispose of the memstate engine");
            await engine.DisposeAsync();

            //////// Replay

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists(JournalFilename));

            Print("WHEN I start up another engine the entire journal at this point should immediately replay all the journaled commands saved to the filesystem");
            engine = await Engine.Start<GraphModel>();

            Print("THEN there should still be three nodes");
            var nodes2 = await engine.Execute(new GetNodes());
            Assert.AreEqual(3, nodes2.Count());

            Print("THEN there should still be four edges");
            var edges2 = await engine.Execute(new GetEdges());
            Assert.AreEqual(4, edges2.Count());

            Print("THEN there should still be one user");
            var usersWithTweets2 = await engine.Execute(new GetUsersWithTweets());
            Assert.AreEqual(1, usersWithTweets2.Count());

            Print("THEN that user should still have one tweet");
            Assert.AreEqual(1, usersWithTweets.First().Get("likes"));

            Print("WHEN I increment likes");
            var tweet2 = await engine.Execute(new IncrementLikes(_tweet.Id));

            Print("THEN there should be one like");
            Assert.AreEqual(2, tweet2.Get("likes"));

            await engine.DisposeAsync();
        }

        private void Print(string text)
        {
            Console.WriteLine(text);
        }
    }
}
