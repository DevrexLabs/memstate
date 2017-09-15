using Memstate.Tcp;
using Xunit;

namespace Memstate.Tests
{
    public class SessionTests
    {
        private readonly Session<TestModel> _protocol;

        public SessionTests()
        {
            var config = new Config();
            var engine = new InMemoryEngineBuilder(config).Build(new TestModel());
            _protocol = new Session<TestModel>(config,engine);
        }

        internal class UnknownMessage : NetworkMessage
        {
            
        }

        [Fact]
        public void UnhandledMessageException()
        {
            Assert.Throws(typeof(string),() =>
            {
                _protocol.Handle(new UnknownMessage());
            });
        }
    }
}