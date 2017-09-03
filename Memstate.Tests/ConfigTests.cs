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

        [Fact]
        public void Default_serializer_type_matches_JsonSerializerAdapter()
        {
            Config config = new Config();
            var expected = typeof(JsonSerializerAdapter).AssemblyQualifiedName;
            _log.WriteLine("Expected: " + expected);
            var actual = config["Serializers:Default"];
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
    }
}