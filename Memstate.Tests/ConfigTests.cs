using Memstate.JsonNet;
using Xunit;
using Xunit.Abstractions;

namespace Memstate.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void Boxed_integer_can_be_cloned()
        {
            Config config = new Config();
            int i = 42;
            var serializer = config.GetSerializer();
            var j = serializer.Clone(i);
            Assert.IsType<int>(j);
        }
    }

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