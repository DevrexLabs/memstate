using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses;
using Memstate.JsonNet;
using NUnit.Framework;
using Wire.Compilation;

namespace Memstate.Examples.GettingStarted._10_QuickStart
{
    // random spikes to work out exactly what works and doesnt with memstate before writing the getting started's
    public class RandomSpikes
    {
        public void How_do_i_get_memstate_to_replay_the_logs()
        {

        }

        public void how_do_I_get_memstate_to_record_events_in_json_or_plain_text()
        {
            // not sure if this is even possible, ignore for now,
            // just get the basics working.
        }

        private void PrintCustomers(Dictionary<CustomerID, Customer> customers)
        {
            Console.WriteLine("Customers");
            Console.WriteLine("---------");

            foreach (var c in customers)
            {
                Console.WriteLine(c.Value);
            }
        }

        [Test]
        public async Task memstate_appears_to_create_one_filebased_journal_file_per_appdomain()
        {
            var settings = new MemstateSettings();
            // can we have a format prefix or format so that we can have something like TestABC_LoyaltyDB
            settings.StreamName = "CAN_YOU_SEE_ME";
            //settings.StorageProvider = typeof(FileStorageProvider).AssemblyQualifiedName;
            //settings.Serializers.Register("newtonsoft.json", _ => new JsonSerializerAdapter(settings));

            var model1 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);
            var customers = await model1.ExecuteAsync(new GetAllCustomersQuery());
            PrintCustomers(customers);

            var id1 = new CustomerID(1);
            var c1 = await model1.ExecuteAsync(new EarnPointsCommand(id1, 200));
            Assert.AreEqual(200, c1.LoyaltyPointBalance);

            customers = await model1.ExecuteAsync(new GetAllCustomersQuery());
            PrintCustomers(customers);

            // lets see what happens if I don't dispose of it?
            // await model1.DisposeAsync();

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
