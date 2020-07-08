using System;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.Test.DispatchProxy
{
    public class ProxyOverloadingTests
    {
        private IModelWithOverloads _db;


        [SetUp]
        public async Task Setup()
        {
            var cfg = Config.Reset();
            cfg.UseInMemoryFileSystem();

            var engine = await Engine.Start<IModelWithOverloads>();
            var client = new LocalClient<IModelWithOverloads>(engine);
            _db = client.GetDispatchProxy();
        }

        [Test]
        public void CanCallNoArgMethod()
        {
            _db.Meth();
           Assert.AreEqual(1, _db.GetCalls());
        }

        [Test]
        public void CanCallOverloadWithAnArgument()
        {
            var inc = _db.Meth(42);
            Assert.AreEqual(43, inc);
        }

        [Test]
        public void CanCallWithParams()
        {
            
            var numbers = new[] {1, 2, 3, 4, 5};
            var sum = numbers.Sum();
            var result = _db.Meth(1,2,3,4,5);
            Assert.AreEqual(sum, result);
        }

        [Test]
        public void CanCallUsingNamedArgs()
        {
            var result = _db.Inc(with: 100, number: 200);
            Assert.AreEqual(300, result);
        }

        [Test]
        public void CanCallWithArrayAsParams()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };
            var sum = numbers.Sum();
            var result = _db.Meth(numbers);
            Assert.AreEqual(sum,result);
        }

        [Test]
        public void CanHandleOptionalArgs()
        {
            var result = _db.Inc(20);
            Assert.AreEqual(21, result);

            result = _db.Inc(20, 5);
            Assert.AreEqual(25,result);
        }


        /// <summary>
        /// It should not be possible to use ref or out args
        /// </summary>
        [Test]
        public void RefArgsNotAllowed()
        {
            Assert.Throws<Exception>(() =>
            {
                Client<IModelWithRefArg> client = null;
                client.GetDispatchProxy();
            });
        }

        [Test]
        public void OutArgsNotAllowed()
        {
            Assert.Throws<Exception>(() =>
                {
                    Client<IModelWithOutArg> client = null;
                    client.GetDispatchProxy();
                });
        }

        internal interface IModelWithOutArg
        {
            void Method(out int a);
        }

        private class ModelWithOutArg  : IModelWithOutArg
        {
            public void Method(out int a)
            {
                a = 42;
            }
        }
        
        private class ModelWithRefArg : IModelWithRefArg
        {
            public void Method(ref int a)
            {
                a = 42;
            }
        }
    }
}