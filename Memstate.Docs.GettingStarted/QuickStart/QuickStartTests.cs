using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using Memstate.Docs.GettingStarted.QuickStart.Queries;
using NUnit.Framework;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    public class QuickStartTests
    {
        private string Filename1 = "smoke_test_with_defaults1";
        private string JournalFile1 = "smoke_test_with_defaults1.journal";

        private string Filename2 = "smoke_test_with_defaults2";
        private string JournalFile2 = "smoke_test_with_defaults2.journal";

        [SetUp]
        [TearDown]
        public void SetupTeardown()
        {
            if (File.Exists(JournalFile1)) File.Delete(JournalFile1);
            if (File.Exists(JournalFile2)) File.Delete(JournalFile2);
        }

        [Test]
         public async Task Simple_end_to_end_getting_started_using_default_wire_serializer_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a LoyaltyDB using default settings");
            Print($"   (using Wire format  & local filesystem storage)");

            var settings = new MemstateSettings { StreamName = Filename1 };
            var model1 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);

            Print("AND I initialise the database with 20 customers, each with 10 loyalty points");
            for (int i = 0; i < 20; i++)
            {
                await model1.ExecuteAsync(new InitCustomer(i + 1, 10));
            }

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists(JournalFile1));

            Print("WHEN customer 5 and customer 12 each earn 190 and 290 loyalty points respectively");

            var c1 = await model1.ExecuteAsync(new EarnPoints(5, 190));
            var c2 = await model1.ExecuteAsync(new EarnPoints(12, 290));

            Print("THEN the balance for them will have increased to 200 and 300 loyalty points for customer 5 and 12 respectively");
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(300, c2.LoyaltyPointBalance);

            Print("WHEN I dispose of the memstate engine");
            await model1.DisposeAsync();

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists(JournalFile1));

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

        [Test]
        public async Task Simple_end_to_end_getting_started_configure_to_use_json_serializer_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a LoyaltyDB using default settings");
            Print($"   (using Json format  & local filesystem storage)");

            var settings = new MemstateSettings { StreamName = Filename2, Serializer = "Json" };

            settings.Serializers.Register("Json", _settings => new JsonNet.JsonSerializerAdapter(_settings));

            var model1 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);

            Print("AND I initialise the database with 20 customers, each with 10 loyalty points");
            for (int i = 0; i < 20; i++)
            {
                await model1.ExecuteAsync(new InitCustomer(i + 1, 10));
            }

            Print("THEN a journal file should now exist on the filesystem");
            Assert.True(File.Exists(JournalFile2));

            Print("WHEN customer 5 and customer 12 each earn 190 and 290 loyalty points respectively");

            var c1 = await model1.ExecuteAsync(new EarnPoints(5, 190));
            var c2 = await model1.ExecuteAsync(new EarnPoints(12, 290));

            Print("THEN the balance for them will have increased to 200 and 300 loyalty points for customer 5 and 12 respectively");
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(300, c2.LoyaltyPointBalance);

            Print("WHEN I dispose of the memstate engine");
            await model1.DisposeAsync();

            Print("THEN a journal file should still exist with all the commands I've played up to now");
            Assert.True(File.Exists(JournalFile2));

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
