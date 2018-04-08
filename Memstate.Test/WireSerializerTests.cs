using NUnit.Framework;

namespace Memstate.Tests
{
    using System.IO;
    using Memstate.Wire;

    [TestFixture]
    public class WireSerializerTests
    {
        [Test]
        public void Can_serialize_simple_POCO()
        {
            var poco = new Poco { Name = "Homer", Age = 35 };
            var settings = new MemstateSettings();
            var serializer = new WireSerializerAdapter(settings);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, poco);

            stream.Position = 0;
            var clone = (Poco) serializer.ReadObject(stream);
            Assert.AreEqual(poco.Name, clone.Name);
            Assert.AreEqual(poco.Age, clone.Age);
        }
    }

    internal class Poco {
        public string Name
        {
            get;
            set;
        }
        public int Age
        {
            get;
            set;
        }
    }
}