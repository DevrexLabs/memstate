using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses;
using NUnit.Framework;
using Wire.Compilation;

namespace Memstate.Examples.GettingStarted._10_QuickStart
{
    public class QuickStart
    {

        // Hi all, here's some important notes on writing tests for .net core and standard,
        // in case you want to create your own tests.
        // https://github.com/nunit/docs/wiki/.NET-Core-and-.NET-Standard

        [Test]
        public async Task Simple_end_to_end_in_memory_sample()
        {
            var fileSystem = new InMemoryFileSystem();

            // hosting the engine 
            // ------------------
            var settings = new MemstateSettings
            {
                FileSystem = fileSystem,

            };

            var model = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);

            // todo Need to use the simple configuration that will result in state being persisted to disk
            // ------------------

            // executing commands
            // ------------------
            var id1 = new CustomerID(1);
            var id2 = new CustomerID(2);

            for (int i = 1; i < 21; i++)
            {
                await model.ExecuteAsync(new EarnPointsCommand(id1, 3));
                await model.ExecuteAsync(new SpendPointsCommand(id1, 1));
                await model.ExecuteAsync(new EarnPointsCommand(id2, 1));
            }

            // at this point we should have customer 1 with 40 points and customer 2 with 2 loyalty points

            // executing queries
            // -----------------
            
            var customers = await model.ExecuteAsync(new GetCustomersQuery(id1, id2));
            Assert.AreEqual(40, customers[id1].LoyaltyPointBalance);
            Assert.AreEqual(20, customers[id2].LoyaltyPointBalance);

            // Now, demonstrate that the model has been persisted to in mem
            // await model.DisposeAsync();

            // create new model
            // model should have replayed all the events, state should be back where we left it
            // assert customer 1.Loyalty = 40
            // assert customer 2.Loyalty = 20
            // customers.length = 20
            // customers[3..20].Loyalty all = 0;
        }


        [Test]
        public async Task simple_end_to_end_with_file_storage()
        {
            // this test should fail, since the engine shoudl replay the journal file! huh!!???
            var model1 = await new EngineBuilder(new MemstateSettings()).BuildAsync<LoyaltyDB>().ConfigureAwait(false);
            var id1 = new CustomerID(1);
            var c1 = await model1.ExecuteAsync(new EarnPointsCommand(id1, 200));
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            await model1.DisposeAsync();

            //// Now, demonstrate that the model has been persisted to disk, shut down current engine, start a new one
            //// new engine will read in the journal and replay all the events, bringing the DB (model) state back to 'live'.
            //// ----------------------------------------------------------------------------------------------------------
            //var model2 = await new EngineBuilder(new MemstateSettings()).BuildAsync<LoyaltyDB>().ConfigureAwait(false);
            //var customers = await model2.ExecuteAsync(new GetCustomersQuery(id1));
            //Assert.AreEqual(200, customers[id1].LoyaltyPointBalance);
        }
    }

    // the journal file takes a few seconds to appear in the bin directory. This is probably by design.
    // need to see what I need to do to flush the test journal before deleting it?
    // with origodb I could specify AsynchronousJournaling to false, see if that's still available?
    // take a look at the smoke tests and see how those are done.

    // https://memstate.io/docs/core-1.0/configuration/engine-configuration/

    //public static class JournalHelper
    //{
    //    public static void DeleteJournal<T>() where T : Model
    //    {
    //        var testBinDir = AppDomain.CurrentDomain.BaseDirectory;
    //        var folder = Path.Combine(testBinDir, typeof(T).Name);
    //        bool exists = Directory.Exists(folder);
    //        if (exists) Directory.Delete(folder, true);
    //    }
    //}
}
