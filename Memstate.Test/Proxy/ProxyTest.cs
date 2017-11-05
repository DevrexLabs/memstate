namespace Memstate.Tests.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ProxyTest
    {
        private readonly ITestModel _proxy;
        private readonly Engine<ITestModel> _engine;

        public ProxyTest()
        {
            var config = new MemstateSettings().WithInmemoryStorage();
            ITestModel model = new TestModel();
            _engine = new EngineBuilder(config).Build(model);
            _proxy = new LocalClient<ITestModel>(_engine).GetDispatchProxy();
        }

        [Fact]
        public void CanSetProperty()
        {
            int expected = _proxy.CommandsExecuted + 1;
            _proxy.MyProperty = 42;
            Assert.Equal(expected, _proxy.CommandsExecuted);
        }

        [Fact]
        public void CanExecuteCommandMethod()
        {
            _proxy.IncreaseNumber();
            Assert.Equal(1, _proxy.CommandsExecuted);
        }

        [Fact]
        public void CanExecuteCommandWithResultMethod()
        {
            Assert.Equal(_proxy.Uppercase("livedb"), "LIVEDB");
            Assert.Equal(1, _proxy.CommandsExecuted);
        }

        [Fact]
        public void ThrowsExceptionOnYieldQuery()
        {
            Assert.ThrowsAny<Exception>(() => _proxy.GetNames().Count());
        }

        [Fact]
        public void CanExecuteQueryMethod()
        {
            var number = _proxy.GetCommandsExecuted();
            Assert.Equal(0, number);
        }

        [Fact]
        public void QueryResultsAreCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomers().First();
            Customer robert2 = _proxy.GetCustomers().First();
            Assert.NotEqual(robert, robert2);
        }

        [Fact]
        public void SafeQueryResultsAreNotCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomersCloned().First();
            Customer robert2 = _proxy.GetCustomersCloned().First();
            Assert.Equal(robert, robert2);
        }

        [Fact]
        public void ResultIsIsolated_attribute_is_recognized()
        {
            var map = MethodMap.MapFor<MethodMapTests.TestModel>();
            var signature = typeof(MethodMapTests.TestModel).GetMethod("GetCustomersCloned").ToString();
            var operationInfo = map.GetOperationInfo(signature);
            Assert.True(operationInfo.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Fact]
        public void GenericQuery()
        {
            var customer = new Customer();
            var clone = _proxy.GenericQuery(customer);
            Assert.NotSame(clone, customer);
            Assert.IsType<Customer>(clone);
        }

        [Fact]
        public void GenericCommand()
        {
            _proxy.GenericCommand(DateTime.Now);
            Assert.Equal(1, _proxy.CommandsExecuted);
        }

        [Fact]
        public void ComplexGeneric()
        {
            double result = _proxy.ComplexGeneric(new KeyValuePair<string, double>("dog", 42.0));
            Assert.Equal(result, 42.0);
            Assert.Equal(1, _proxy.CommandsExecuted);
        }

        [Fact]
        public void Indexer()
        {
            _proxy.AddCustomer("Homer");
            Assert.Equal(1, _proxy.CommandsExecuted);

            var customer = _proxy[0];
            Assert.Equal("Homer", customer.Name);

            customer.Name = "Bart";
            _proxy[0] = customer;
            Assert.Equal(2, _proxy.CommandsExecuted);
            var customers = _proxy.GetCustomers();
            Assert.True(customers.Single().Name == "Bart");
        }

        [Fact]
        public void DefaultArgs()
        {
            var result = _proxy.DefaultArgs(10, 10);
            Assert.Equal(62, result);

            result = _proxy.DefaultArgs(10, 10, 10);
            Assert.Equal(30, result);
        }

        [Fact]
        public void NamedArgs()
        {
            var result = _proxy.DefaultArgs(b: 4, a: 2);
            Assert.Equal(48, result);
        }

        [Fact]
        public void ExplicitGeneric()
        {
            var dt = _proxy.ExplicitGeneric<DateTime>();
            Assert.IsType<DateTime>(dt);
            Assert.Equal(default(DateTime), dt);
        }

        [Fact]
        public void Proxy_throws_InnerException()
        {
            Assert.Throws<CommandAbortedException>(() =>
            {
                _proxy.ThrowCommandAborted();
            });
        }
    }
}
