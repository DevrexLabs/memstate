using System.Threading.Tasks;
using Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses;
using NUnit.Framework;

namespace Memstate.Examples.GettingStarted._10_QuickStart
{
    public class QuickStart
    {
        [Test]
        public async Task Simple_end_to_end_sample()
        {
            return;

            var fileSystem = new InMemoryFileSystem();

            // hosting the engine 
            // ------------------
            var settings = new MemstateSettings
            {
                FileSystem = fileSystem
            };

            var model = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);

            // todo Need to use the simple configuration that will result in state being persisted to disk
            // ------------------

            // executing commands
            // ------------------
            var id1 = new CustomerID(1);
            var id2 = new CustomerID(2);

            for (int i = 1; i < 20; i++)
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
            Assert.AreEqual(40, customers[id2].LoyaltyPointBalance);

            // Now, demonstrate that the model has been persisted to disk
            // await model.DisposeAsync();

            // create new model
            // model should have replayed all the events, state should be back where we left it
            // assert customer 1.Loyalty = 40
            // assert customer 2.Loyalty = 20
            // customers.length = 20
            // customers[3..20].Loyalty all = 0;
        }
    }
}
