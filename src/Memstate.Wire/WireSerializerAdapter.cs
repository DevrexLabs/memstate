using System.IO;
using Hyperion;

namespace Memstate.Wire
{
    public class WireSerializerAdapter : BinarySerializer
    {
        private readonly Serializer _serializer;

        public WireSerializerAdapter()
        {
            var options = new SerializerOptions(versionTolerance: false, preserveObjectReferences: true);

            _serializer = new Serializer(options);
        }

        public override void WriteObject(Stream stream, object @object)
        {
            if (@object is JournalRecord[])
            {
                foreach(var record in (@object as JournalRecord[]))
                {
                    _serializer.Serialize(record, stream);
                }
            }
            else _serializer.Serialize(@object, stream);
        }

        public override object ReadObject(Stream stream)
        {
            return _serializer.Deserialize(stream);
        }
    }
}