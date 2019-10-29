using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

using Memstate.JsonNet;
using Memstate.Test.EventfulTestDomain;
using Memstate.Wire;
using System;

namespace Memstate.Test
{
    [TestFixture]
    public class SerializationTests
    {
        private static IEnumerable<ISerializer> Serializers()
        {
            yield return new WireSerializerAdapter();
            yield return new JsonSerializerAdapter();
            yield return new BinaryFormatterAdapter();
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

        [Test, TestCaseSource(nameof(Serializers))]
        public void Can_serialize_poco_with_readonly_fields(ISerializer serializer)
        {
            var poco = new PocoWithReadonlyFields("homer");
            var clone = serializer.Clone(poco);
            Assert.AreEqual("homer", clone.Name);
        }

        [Test, TestCaseSource(nameof(Serializers))]
        public void Can_serialize_poco_with_no_default_constructor(ISerializer serializer)
        {
            var poco = new PocoWithPrivateSettersAndNoDefaultConstructor("homer");
            var clone = serializer.Clone(poco);
            Assert.AreEqual("homer", clone.Name);
        }

    }

    [Serializable]
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

    [Serializable]
    public class PocoWithPrivateSettersAndNoDefaultConstructor
    {
        public string Name { get; private set; }

        public PocoWithPrivateSettersAndNoDefaultConstructor(string name)
        {
            Name = name;
        }
    }

    [Serializable]
    public class PocoWithReadonlyFields
    {
        public readonly string Name;

        public PocoWithReadonlyFields(string name)
        {
            Name = name;
        }
    }
}