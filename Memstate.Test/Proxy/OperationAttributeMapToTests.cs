namespace Memstate.Tests.DispatchProxy
{
    using System.Linq;
    using Xunit;

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

        [Fact]
        public void MapsToCommand()
        {
            var settings  = new MemstateSettings();
            settings.FileSystem = new InMemoryFileSystem();
            var storageProvider = settings.CreateStorageProvider();
            var builder = new EngineBuilder(settings, storageProvider);
            var engine = builder.Build<ITestModel>(new TestModel());
            var client = new LocalClient<ITestModel>(engine);
            var proxy = client.GetDispatchProxy();
            proxy.SetCustomer(new Customer());
            var journalEntry = storageProvider.CreateJournalReader().GetRecords().FirstOrDefault();
            Assert.NotNull(journalEntry);
            Assert.IsType(typeof(SetCustomerCommand), journalEntry.Command);
        }
    }
}
