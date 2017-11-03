using System.Collections.Generic;
using System.IO;
using Wire;

namespace Memstate.Wire
{
    public class WireSerializerAdapter : ISerializer
    {
        private readonly Serializer _serializer;

        public WireSerializerAdapter(MemstateSettings config)
        {
            var options = new SerializerOptions(versionTolerance: false, preserveObjectReferences: true);
            _serializer = new Serializer(options);
        }

        public void WriteObject(Stream stream, object @object)
        {
            _serializer.Serialize(@object, stream);
        }

        public object ReadObject(Stream stream)
        {
            return _serializer.Deserialize(stream);
        }

        public IEnumerable<T> ReadObjects<T>(Stream stream)
        {
            while (stream.Position < stream.Length - 1)
            {
                yield return _serializer.Deserialize<T>(stream);
            }
        }
    }
}
