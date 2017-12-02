using System.IO;
using Memstate.JsonNet;
using Xunit;

namespace Memstate.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void Int32_can_be_cloned()
        {
            int i = 42;
            var serializer = new JsonSerializerAdapter();
            var stream = new MemoryStream();
            serializer.WriteObject(stream, i);
            stream.Position = 0;
            object o = serializer.ReadObject(stream);
            Assert.IsType<int>(o);
        }

        [Fact]
        public void Int64_can_be_cloned()
        {
            long i = 42;
            var serializer = new JsonSerializerAdapter();
            var stream = new MemoryStream();
            serializer.WriteObject(stream, i);
            stream.Position = 0;
            object o = serializer.ReadObject(stream);
            Assert.IsType<long>(o);
        }

        [Fact]
        public void Float_can_be_cloned()
        {
            float i = 42.1F;
            var serializer = new JsonSerializerAdapter();
            var stream = new MemoryStream();
            serializer.WriteObject(stream, i);
            stream.Position = 0;
            object o = serializer.ReadObject(stream);
            Assert.IsType<float>(o);
        }

        [Fact]
        public void Double_can_be_cloned()
        {
            double i = 42.10;
            var serializer = new JsonSerializerAdapter();
            var stream = new MemoryStream();
            serializer.WriteObject(stream, i);
            stream.Position = 0;
            object o = serializer.ReadObject(stream);
            Assert.IsType<double>(o);
        }
    }
}