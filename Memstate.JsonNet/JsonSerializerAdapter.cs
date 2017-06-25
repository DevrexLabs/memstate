using System.IO;
using Memstate.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Memstate.JsonNet
{
    public class JsonSerializerAdapter : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonSerializerAdapter()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableAttribute = true,
                    SerializeCompilerGeneratedMembers = true
                },
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            _serializer = JsonSerializer.Create(settings);
        }

        public object Deserialize(Stream serializationStream)
        {
            var reader = new JsonTextReader(new StreamReader(serializationStream));

            return _serializer.Deserialize(reader);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            var writer = new JsonTextWriter(new StreamWriter(serializationStream));

            _serializer.Serialize(writer, graph);

            writer.Flush();
        }
    }
}