using System;
using System.IO;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using Memstate.Docs.GettingStarted.QuickStart.Queries;
using NUnit.Framework;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    public class QuickStartTests
    {
        private string WireFileName = "smoke_test_with_defaults_wire";
        private string WireJournalFile = "smoke_test_with_defaults_wire.journal";

        private string JsonFileName = "smoke_test_with_defaults_json";
        private string JsonJournalFile = "smoke_test_with_defaults_json.journal";

        [SetUp]
        [TearDown]
        public void SetupTeardown()
        {
            if (File.Exists(WireJournalFile)) File.Delete(WireJournalFile);
            if (File.Exists(JsonJournalFile)) File.Delete(JsonJournalFile);
            Config.Reset();
        }

        [Test]
         public async Task Simple_end_to_end_getting_started_using_default_wire_serializer_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a LoyaltyDB using default settings");
            Print("   (using Wire format  & local filesystem storage)");
            var config = Config.Current;
            config.SerializerName = "Wire";
            var settings = config.GetSettings<EngineSettings>();
            settings.StreamName = WireFileName;
            var engine = await Engine.Start<LoyaltyDB>();
            
            Print("AND I initialise the database with 20 customers, each with 10 loyalty points");
            for (int i = 0; i < 20; i++)
            {
                await engine.Execute(new InitCustomer(i + 1, 10));
            }

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists(WireJournalFile));

            Print("WHEN customer 5 and customer 12 each earn 190 and 290 loyalty points respectively");
            var c1 = await engine.Execute(new EarnPoints(5, 190));
            var c2 = await engine.Execute(new EarnPoints(12, 290));

            Print("THEN the balance for them will have increased to 200 and 300 loyalty points for customer 5 and 12 respectively");
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(300, c2.LoyaltyPointBalance);

            Print("WHEN I dispose of the memstate engine");
            await engine.DisposeAsync();

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists(WireJournalFile));

            Print("WHEN I start up another engine");
            engine = await Engine.Start<LoyaltyDB>();

            Print("THEN the entire journal at this point should immediately replay all the journaled commands saved to the filesystem");
            var allCustomers = await engine.Execute(new GetCustomers());

            Print("AND the database should be restored to the exact same state it was after the last command was executed");
            Assert.AreEqual(20, allCustomers.Count);
            Assert.AreEqual(200, allCustomers[5].LoyaltyPointBalance);
            Assert.AreEqual(300, allCustomers[12].LoyaltyPointBalance);

            await engine.DisposeAsync();
        }

        // rules for Command using Json serialisation
        // ----------------------------------
        // Commands must have public empty constructors 
        // For now, properties must have { get; set; } or at minimum { get; private set; }
        // Note : we are working hard to make everything as far as possible serializer agnostic so expect this restriction to be lifted shortly, woo hoo!

        [Test]
        public async Task Simple_end_to_end_getting_started_configure_to_use_json_serializer_with_filesytem_storage()
        {

            Print("GIVEN I start a new Memstate engine for a LoyaltyDB using default settings");
            Print($"   (using Json format & local filesystem storage)");

            var config = Config.Current;
            config.SerializerName = "NewtonSoft.Json";
            var settings = config.GetSettings<EngineSettings>();
            settings.StreamName = JsonFileName;
            var engine = await Engine.Start<LoyaltyDB>();

            Print("AND I initialise the database with 2 customers, each with 10, and 20 loyalty points");
            await engine.Execute(new InitCustomer(10, 10));
            await engine.Execute(new InitCustomer(20, 20));

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists(JsonJournalFile));

            Print("WHEN customer 10 transfers 5 points to customer 20");
            var result = await engine.Execute(new TransferPoints(10, 20, 5));

            Print("THEN the new balance for them will be 5 and 25 respectively");
            Assert.AreEqual(5, result.Sender.LoyaltyPointBalance);
            Assert.AreEqual(25, result.Recipient.LoyaltyPointBalance);

            Print("WHEN I dispose of the memstate engine");
            await engine.DisposeAsync();

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists(JsonJournalFile));

            Print("WHEN I start up another engine");
            engine = await Engine.Start<LoyaltyDB>();

            Print("THEN the entire journal at this point should immediately replay all the journaled commands saved to the filesystem");
            var allCustomers = await engine.Execute(new GetCustomers());

            Print("AND the database should be restored to the exact same state it was after the last command was executed");
            Assert.AreEqual(2, allCustomers.Count);
            Assert.AreEqual(5, allCustomers[10].LoyaltyPointBalance);
            Assert.AreEqual(25, allCustomers[20].LoyaltyPointBalance);

            await engine.DisposeAsync();
        }

        private void Print(string text)
        {
            Console.WriteLine(text);
        }
    }
}
