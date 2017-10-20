
using System.Linq;
using FakeItEasy;
using Xunit;

namespace Memstate.Tests.DispatchProxy
{
    public class OperationAttributeMapToTests
    {
        internal class SetCustomerCommand : Command<ITestModel>
        {
            public Customer Customer { get; }

            public SetCustomerCommand(Customer customer)
            {
                Customer = customer;
            }

            public override void Execute(ITestModel model)
            {
                model.SetCustomer(Customer);
            }
        }

        internal interface ITestModel
        {
            Customer Customer { get; }

            [Command(MapTo = typeof(SetCustomerCommand))]
            void SetCustomer(Customer c);
        }

        internal class TestModel : ITestModel
        {
            public Customer Customer { get; private set; }

            public void SetCustomer(Customer c)
            {
                Customer = Customer;
            }
        }

        [Fact]
        public void MapsToCommand()
        {
            var config  = new Config();
            var commandStore = new InMemoryCommandStore(config);
            var builder = new InMemoryEngineBuilder(config, commandStore);
            var engine = builder.Build<ITestModel>(new TestModel());
            var client = new LocalClient<ITestModel>(engine);
            var proxy = client.GetDispatchProxy();
            proxy.SetCustomer(new Customer());
            var journalEntry = commandStore.GetRecords().FirstOrDefault();
            Assert.NotNull(journalEntry);
            Assert.IsType(typeof(SetCustomerCommand), journalEntry.Command);
        }
    }
}