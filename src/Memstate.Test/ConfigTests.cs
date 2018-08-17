using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void CanGetDefaultSerializer()
        {
            var config = new Config();
            var serializer = config.CreateSerializer();
            Assert.NotNull(serializer);
        }
    }
}