

namespace Memstate.Tests.DispatchProxy
{
    public class OperationAttributeMapToTests
    {
        internal class TestEngine : Engine<TestModel>
        {
            //the query or command that was executed
            public object Executee;

            public TResult Execute<TResult>(Query<TestModel, TResult> query)
            {
                Executee = query;
                return default(TResult);
            }

            public void Execute(Command<TestModel> command)
            {
                Executee = command;
            }

            public TResult Execute<TResult>(Command<TestModel, TResult> command)
            {
                Executee = command;
                return default(TResult);
            }

            public object Execute(Command command)
            {
                Executee = command;
                return null;
            }

            public object Execute(Query query)
            {
                Executee = query;
                return null;
            }
        }

        internal class SetCustomerCommand : Command<TestModel>
        {
            public readonly Customer Customer;
            public SetCustomerCommand(Customer customer)
            {
                Customer = customer;
            }

            public override void Execute(TestModel model)
            {
                model.SetCustomer(Customer);
            }
        }
        internal class TestModel : Model
        {
            public Customer Customer { get; private set; }

            [Command(MapTo = typeof(SetCustomerCommand))]
            public void SetCustomer(Customer c)
            {
                Customer = Customer;
            }
        }

        [Test]
        public void MapsToCommand()
        {
            var engine = new TestEngine();
            var proxy = (TestModel) new Proxy<TestModel>(engine).GetTransparentProxy();
            proxy.SetCustomer(new Customer());
            Assert.AreEqual(typeof(SetCustomerCommand), engine.Executee.GetType());
        }
    }
}