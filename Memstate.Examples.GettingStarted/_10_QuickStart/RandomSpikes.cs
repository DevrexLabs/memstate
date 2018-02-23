using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses;
using NUnit.Framework;

namespace Memstate.Examples.GettingStarted._10_QuickStart
{
    // random spikes to work out exactly what works and doesnt with memstate before writing the getting started's
    public class RandomSpikes
    {

        [Test]
        public async Task Smoke_test_with_default_HostFileSystem_and_Wire_Serializer()
        {
            var settings = new MemstateSettings();
            settings.StreamName = "Smoke103";
            var model1 = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);

            var customers = await model1.ExecuteAsync(new GetAllCustomersQuery());

            Console.WriteLine("loading the database and printing existing customers, should be none on first run, and some on second");
            //----------------------------------------------------------------
            PrintCustomers(customers);

            Console.WriteLine("initialising 10 customers with 10 points each");
            //----------------------------------------------------------------
            for (int i = 0; i < 40; i++)
            {
                await model1.ExecuteAsync(new InitCustomerCommand(new CustomerID(i+1), 10));
            }

            Console.WriteLine("now lets have customer 5 and customer 12 each earn 190 points respectively");
            //--------------------------------------------------------------------------------------------

            var c1 = await model1.ExecuteAsync(new EarnPointsCommand(new CustomerID(5), 190));
            var c2 = await model1.ExecuteAsync(new EarnPointsCommand(new CustomerID(12), 190));

            Console.WriteLine("Assert that the balance has not increased to 200 points for customer 5 and 12 (the second this time you run this unit test it should fail!)");
            //-----------------------------------------------------------------------------------------------
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(200, c2.LoyaltyPointBalance);
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

    }


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
