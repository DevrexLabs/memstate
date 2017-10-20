using Memstate.JsonNet;
using Xunit;
using Xunit.Abstractions;

namespace Memstate.Tests
{
    public class ConfigTests
    {
        private ITestOutputHelper _log;

        public ConfigTests(ITestOutputHelper log)
        {
            _log = log;
        }
        
        [Fact]
        public void CanGetDefaultSerializer()
        {
            Config config = new Config();
            var serializer = config.GetSerializer();
            Assert.NotNull(serializer);
        }
    }
}