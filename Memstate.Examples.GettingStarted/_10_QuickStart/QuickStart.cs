using System.Threading.Tasks;
using Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses;
using Xunit;

namespace Memstate.Examples.GettingStarted._10_QuickStart
{
    public class QuickStart
    {
        [Fact]
        public async Task Simple_end_to_end_sample()
        {
            var fileSystem = new InMemoryFileSystem();

            // hosting the engine 
            // ------------------
            var settings = new MemstateSettings
            {
                FileSystem = fileSystem
            };

            var model = await new EngineBuilder(settings).BuildAsync<CustomerModel>().ConfigureAwait(false);

            // todo Need to use the simple configuration that will result in state being persisted to disk
            // ------------------

            // executing commands
            // ------------------
            for (int i = 1; i < 20; i++)
            { 
                var id1 = new CustomerID(1);
                var id2 = new CustomerID(2);
                await model.ExecuteAsync(new EarnPoints(id1, 3));
                await model.ExecuteAsync(new SpendPoints(id1, 1));
                await model.ExecuteAsync(new EarnPoints(id2, 1));
            }

            // at this point we should have customer 1 with 40 points and customer 2 with 2 loyalty points

            // executing queries
            // -----------------
            // prove statement above

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
