using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using Memstate.Docs.GettingStarted.QuickStart.Queries;
using Memstate.JsonNet;
using NUnit.Framework;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    public class QuickStartTests
    {

        [SetUp]
        [TearDown]
        public void Setup()
        {
            File.Delete("wire-example.journal");
            File.Delete("json-example.journal");
        }

        [Test]
        public async Task Simple_end_to_end_getting_started_using_default_settings_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a LoyaltyDB using default settings");
            Print("   (using Wire format  & local filesystem storage)");
            var settings = new MemstateSettings { StreamName = "wire-example" };
            var model1 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);
            Print("AND I initialise the database with 20 customers, each with 10 loyalty points");
            for (int i = 0; i < 20; i++)
            {
                await model1.ExecuteAsync(new InitCustomer(i + 1, 10));
            }

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists("wire-example.journal"));

            Print("WHEN customer 5 and customer 12 each earn 190 and 290 loyalty points respectively");

            var c1 = await model1.ExecuteAsync(new EarnPoints(5, 190));
            var c2 = await model1.ExecuteAsync(new EarnPoints(12, 290));

            Print("THEN the balance for them will have increased to 200 and 300 loyalty points for customer 5 and 12 respectively");
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(300, c2.LoyaltyPointBalance);

            Print("WHEN I dispose of the memstate engine");
            await model1.DisposeAsync();

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists("wire-example.journal"));

            Print("WHEN I start up another engine");
            var model2 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);

            Print("THEN the entire journal at this point should immediately replay all the journaled commands saved to the filesystem");
            var allCustomers = await model2.ExecuteAsync(new GetCustomers());

            Print("AND the database should be restored to the exact same state it was after the last command was executed");
            Assert.AreEqual(20, allCustomers.Count);
            Assert.AreEqual(200, allCustomers[5].LoyaltyPointBalance);
            Assert.AreEqual(300, allCustomers[12].LoyaltyPointBalance);

            await model2.DisposeAsync();
        }

        // now that you've seen the basics, we'll move a bit quicker and skip all the printing.

        [Test]
        public async Task Simple_end_to_end_with_human_readable_json_journal_file()
        {
            Print("Given a memstate db configured to write journal extries as json, 1 line of json per journal entry");
            var settings = new MemstateSettings { StreamName = "json-example" };
            settings.Serializers.Register("a-unique-key", _ => new JsonSerializerAdapter(settings));
            settings.Serializer = "a-unique-key";
            var model1 = await new EngineBuilder(settings).BuildAsync<LedgerDB>();

            Print("When I run a few commands");
            await model1.ExecuteAsync(new InitAccount(accountNumber: 1, openingBalance: 100.11M, currency: '£'));
            await model1.ExecuteAsync(new InitAccount(accountNumber: 2, openingBalance: 200.22M, currency: '£'));
            await model1.ExecuteAsync(new Transfer(fromAccount: 1, toAccount: 2, amount: 0.01M, currency:'£'));
            var accounts = await model1.ExecuteAsync(new GetAccounts());
            await model1.DisposeAsync();
            accounts.ForEach(Console.WriteLine);

            Print("then the journal file should exist");
            Assert.True(File.Exists("json-example.journal"));

            Print("And the journal file should have saved the entries as text, 1 line per entry");
            var lines = File.ReadAllLines("json-example.journal");
            Assert.AreEqual(3, lines.Length);
            StringAssert.Contains("M100.11", lines[0]);
            StringAssert.Contains("M200.22", lines[1]);
            StringAssert.Contains("M0.01", lines[2]);

            // to open the journal file in an editor, delete the line below or set a breakpoint
            // journal file will be located in Memstate.Docs.GettingStarted\bin\Debug\netcoreapp2.0\json-example.journal
        }

        private void Print(string text)
        {
            Console.WriteLine(text);
        }

    }
}

// some useful references
// ----------------------

// Hi all, here's some important notes on writing tests for .net core and standard,
// in case you want to create your own tests.
// https://github.com/nunit/docs/wiki/.NET-Core-and-.NET-Standard

// Online Json viewer and formatter. 
// https://jsonformatter.curiousconcept.com/
// If you're looking at large files with lots of embedded json. 
// will pretty print and neatly indent json that's been formatted as a single line

// official website for Wire (A high performance polymorphic serializer for the .NET framework.)
// https://github.com/rogeralsing/Wire
// Wire is the default file formatter that is used with memstate.
