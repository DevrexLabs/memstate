using System;
using System.Linq;
using Xunit;

namespace Memstate.Tests.DispatchProxy
{
    public class ProxyOverloadingTests
    {
        private ModelWithOverloads _db;

        public ProxyOverloadingTests()
        {
        }

        [Fact]
        public void CanCallNoArgMethod()
        {
            _db.Meth();
           Assert.Equal(_db.GetCalls(), 1);
        }

        [Fact]
        public void CanCallOverloadWithAnArgument()
        {
            var inc = _db.Meth(42);
            Assert.Equal(43, inc);
        }

        [Fact]
        public void CanCallWithParams()
        {
            
            var numbers = new[] {1, 2, 3, 4, 5};
            var sum = numbers.Sum();
            var result = _db.Meth(1,2,3,4,5);
            Assert.Equal(sum, result);
        }

        [Fact]
        public void CanCallUsingNamedArgs()
        {
            var result = _db.Inc(with: 100, number: 200);
            Assert.Equal(300, result);
        }

        [Fact]
        public void CanCallWithArrayAsParams()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };
            var sum = numbers.Sum();
            var result = _db.Meth(numbers);
            Assert.Equal(sum,result);
        }

        [Fact]
        public void CanHandleOptionalArgs()
        {
            var result = _db.Inc(20);
            Assert.Equal(21, result);

            result = _db.Inc(20, 5);
            Assert.Equal(25,result);
        }


        /// <summary>
        /// It should not be possible to use ref or out args
        /// </summary>
        [Fact]
        public void RefArgsNotAllowed()
        {
            Assert.Throws<Exception>(() =>
            {
                Client<ModelWithRefArg> client = null;
                var proxy = client.GetProxy();
            });
        }

        [Fact]
        public void OutArgsNotAllowed()
        {
            Assert.Throws<Exception>(() => new ModelProxy<ModelWithOutArg>());
        }


        private class ModelWithOutArg 
        {
            public void Method(out int a)
            {
                a = 42;
            }
        }
        
        private class ModelWithRefArg 
        {
            public void Method(ref int a)
            {
                a = 42;
            }
        }
    }
}