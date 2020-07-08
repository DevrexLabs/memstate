using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.Test.Proxy
{
    [TestFixture]
    public class OperationAttributeMapToTests
    {
        private class SetCustomerCommand : Command<ITestModel>
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

        [Test, Ignore("hangs on call to proxySetCustomer")]
        public async Task MapsToCommand()
        {
            //Arrange
            var config = Config.Reset();
            config.UseInMemoryFileSystem();
            var settings = Config.Current.GetSettings<EngineSettings>();
            settings.WithRandomSuffixAppendedToStreamName();
            
            var storageProvider = config.GetStorageProvider();
            var client = await Client.For<ITestModel>();
            var proxy = client.GetDispatchProxy();

            //Act
            proxy.SetCustomer(new Customer());

            //release the lock on the journal
            await client.DisposeAsync();
            var journalEntry = storageProvider
                .CreateJournalReader()
                .ReadRecords()
                .Skip(1) //First command is a control command
                .FirstOrDefault();

            // If MapTo is correct, a SetCustomerCommand will be written to the journal
            // if not, then a ProxyCommand will be written
            Assert.NotNull(journalEntry);
            Assert.IsInstanceOf<SetCustomerCommand>(journalEntry.Command);
        }
    }
}
