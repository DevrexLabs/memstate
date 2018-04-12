using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void CanGetDefaultSerializer()
        {
            MemstateSettings config = new MemstateSettings();
            var serializer = config.CreateSerializer();
            Assert.NotNull(serializer);
        }
    }
}