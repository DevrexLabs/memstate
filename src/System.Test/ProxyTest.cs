using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Test;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate.Test.Proxy
{

    [TestFixtureSource(typeof(TestConfigurations),  nameof(TestConfigurations.All))]

    public class ProxyTest
    {
        private ITestModel _proxy;
        private readonly Config _config;
        private Engine<ITestModel> _engine;

        public ProxyTest(Config config)
        {
            this._config = config;
        }

        [SetUp]
        public async Task Setup()
        {
            _engine = await Engine.Start<ITestModel>(_config);
            _proxy = new LocalClient<ITestModel>(_engine).GetDispatchProxy();
        }

        [TearDown]
        public Task TearDown() => _engine.DisposeAsync();

        [Test, Ignore("newtonsoft Int64 to Int32 issue")]
        public void CanSetProperty()
        {
            int expected = _proxy.CommandsExecuted + 1;
            _proxy.MyProperty = 42;
            Assert.AreEqual(expected, _proxy.CommandsExecuted);
        }

        [Test]
        public void CanExecuteCommandMethod()
        {
            _proxy.IncreaseNumber();
            Assert.AreEqual(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void CanExecuteCommandWithResultMethod()
        {
            Assert.AreEqual("MEMSTATE", _proxy.Uppercase("memstate"));
            Assert.AreEqual(1, _proxy.CommandsExecuted);
        }

        [Test]
        [Ignore("Isolation is yet to be implemented")]
        public void ThrowsExceptionOnYieldQuery()
        {
            Assert.Throws<Exception>(() => _proxy.GetNames().Count());
        }

        [Test]
        public void CanExecuteQueryMethod()
        {
            var number = _proxy.GetCommandsExecuted();
            Assert.AreEqual(0, number);
        }

        [Test]
        [Ignore("Isolation is yet to be implemented")]
        public void QueryResultsAreCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomers().First();
            Customer robert2 = _proxy.GetCustomers().First();
            Assert.AreNotEqual(robert, robert2);
        }

        [Test]
        public void SafeQueryResultsAreNotCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomersCloned().First();
            Customer robert2 = _proxy.GetCustomersCloned().First();
            Assert.AreEqual(robert, robert2);
        }

        [Test]
        [Ignore("Isolation is yet to be designed and implemented")]
        public void Query_result_is_cloned()
        {
            var customer = new Customer();
            var clone = _proxy.GenericQuery(customer);
            Assert.AreNotSame(clone, customer);
            Assert.IsInstanceOf<Customer>(clone);
        }

        [Test]
        public void GenericCommand()
        {
            _proxy.GenericCommand(DateTime.Now);
            Assert.AreEqual(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void ComplexGeneric()
        {
            double result = _proxy.ComplexGeneric(new KeyValuePair<string, double>("dog", 42.0));
            Assert.AreEqual(42.0, result, 0.0001);
            Assert.AreEqual(1, _proxy.CommandsExecuted);
        }

        [Test, Ignore("newtonsoft Int64 to Int32 issue")]
        public void Indexer()
        {
            _proxy.AddCustomer("Homer");
            Assert.AreEqual(1, _proxy.CommandsExecuted);

            var customer = _proxy[0];
            Assert.AreEqual("Homer", customer.Name);

            customer.Name = "Bart";
            _proxy[0] = customer;
            Assert.AreEqual(2, _proxy.CommandsExecuted);
            var customers = _proxy.GetCustomers();
            Assert.True(customers.Single().Name == "Bart");
        }

        [Test]
        public void DefaultArgs()
        {
            var result = _proxy.DefaultArgs(10, 10);
            Assert.AreEqual(62, result);

            result = _proxy.DefaultArgs(10, 10, 10);
            Assert.AreEqual(30, result);
        }

        [Test]
        public void NamedArgs()
        {
            var result = _proxy.DefaultArgs(b: 4, a: 2);
            Assert.AreEqual(48, result);
        }

        [Test]
        public void ExplicitGeneric()
        {
            var dt = _proxy.ExplicitGeneric<DateTime>();
            Assert.IsInstanceOf<DateTime>(dt);
            Assert.AreEqual(default(DateTime), dt);
        }

        [Test]
        public void Proxy_throws_InnerException()
        {
            Assert.Throws<CommandAbortedException>(() =>
            {
                _proxy.ThrowCommandAborted();
            });
        }
    }
}
