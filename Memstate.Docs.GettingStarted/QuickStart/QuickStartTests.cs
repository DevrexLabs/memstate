﻿using System;
using System.IO;
using System.Threading.Tasks;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using Memstate.Docs.GettingStarted.QuickStart.Queries;
using NUnit.Framework;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    public class QuickStartTests
    {
        private string Filename = "smoke_test_with_defaults";
        private string JournalFile = "smoke_test_with_defaults.journal";

        [SetUp]
        [TearDown]
        public void Setup()
        {
            if (File.Exists(JournalFile)) File.Delete(JournalFile);
        }

        // Hi all, here's some important notes on writing tests for .net core and standard,
        // in case you want to create your own tests.
        // https://github.com/nunit/docs/wiki/.NET-Core-and-.NET-Standard

        [Test]
        public async Task Simple_end_to_end_getting_started_using_default_settings_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a LoyaltyDB using default settings");
            Print("   (using Wire format  & local filesystem storage)");
            var settings = new MemstateSettings { StreamName = Filename };
            var model1 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);
            Print("AND I initialise the database with 20 customers, each with 10 loyalty points");
            for (int i = 0; i < 20; i++)
            {
                await model1.ExecuteAsync(new InitCustomer(i + 1, 10));
            }

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists(JournalFile));

            Print("WHEN customer 5 and customer 12 each earn 190 and 290 loyalty points respectively");

            var c1 = await model1.ExecuteAsync(new EarnPoints(5, 190));
            var c2 = await model1.ExecuteAsync(new EarnPoints(12, 290));

            Print("THEN the balance for them will have increased to 200 and 300 loyalty points for customer 5 and 12 respectively");
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(300, c2.LoyaltyPointBalance);

            Print("WHEN I dispose of the memstate engine");
            await model1.DisposeAsync();

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists(JournalFile));

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

        private void Print(string text)
        {
            Console.WriteLine(text);
        }


    }
}
