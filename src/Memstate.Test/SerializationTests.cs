using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

using Memstate.JsonNet;
using Memstate.Test.EventfulTestDomain;
using Memstate.Wire;

namespace Memstate.Test
{
    [TestFixture]
    public class SerializationTests
    {
        private static IEnumerable<ISerializer> Serializers()
        {
            var settings = new MemstateSettings();
            yield return new WireSerializerAdapter(settings);
            yield return new JsonSerializerAdapter(settings);
        }

        [Test, TestCaseSource(nameof(Serializers))]
        public void Can_serialize_simple_POCO(ISerializer serializer)
        {
            var poco = new Poco { Name = "Homer", Age = 35 };
            var stream = new MemoryStream();
            serializer.WriteObject(stream, poco);

            stream.Position = 0;
            var clone = (Poco) serializer.ReadObject(stream);
            Assert.AreEqual(poco.Name, clone.Name);
            Assert.AreEqual(poco.Age, clone.Age);
        }

        [Test, TestCaseSource(nameof(Serializers))]
        public void Command_retains_id_when_deserialized(ISerializer serializer)
        {
            var command = new Create("dummy");
            var clone = serializer.Clone(command);
            Assert.AreEqual(command.Id, clone.Id);
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