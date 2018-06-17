using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate.Test.DispatchProxy
{
    [TestFixture]
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
                Customer = c;
            }
        }

        [Test]
        public async Task MapsToCommand()
        {
            //Arrange
            var settings  = new MemstateSettings();
            settings.FileSystem = new InMemoryFileSystem();
            var storageProvider = settings.CreateStorageProvider();
            var builder = new EngineBuilder(settings, storageProvider);
            var engine = builder.Build<ITestModel>(new TestModel()).Result;
            var client = new LocalClient<ITestModel>(engine);
            var proxy = client.GetDispatchProxy();

            //Act
            proxy.SetCustomer(new Customer());

            //release the lock on the journal
            await engine.DisposeAsync();
            var journalEntry = storageProvider.CreateJournalReader().GetRecords().FirstOrDefault();

            // If MapTo is correct, a SetCustomerCommand will be written to the journal
            // if not, then a ProxyCommand will be written
            Assert.NotNull(journalEntry);
            Assert.IsInstanceOf<SetCustomerCommand>(journalEntry.Command);
        }
    }
}
