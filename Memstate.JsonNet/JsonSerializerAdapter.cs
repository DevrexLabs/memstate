using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System;

namespace Memstate.JsonNet
{
    public class JsonSerializerAdapter : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonSerializerAdapter(MemstateSettings config = null)
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
                MissingMemberHandling = MissingMemberHandling.Ignore,
                CheckAdditionalContent = false
            };

            _serializer = JsonSerializer.Create(settings);
        }

        public object ReadObject(Stream serializationStream)
        {
            var streamReader = new StreamReader(serializationStream);
            var line = streamReader.ReadLine();
            var reader = new JsonTextReader(new StringReader(line));
            return _serializer.Deserialize(reader);
        }

        public IEnumerable<T> ReadObjects<T>(Stream stream)
        {
            using (var reader = new JsonTextReader(new StreamReader(stream))
            {
                SupportMultipleContent = true
            })
            {
                while (reader.Read())
                {
                    yield return _serializer.Deserialize<T>(reader);
                }
            }
        }

        public void WriteObject(Stream serializationStream, object @object)
        {
            var streamWriter = new StreamWriter(serializationStream);
            
            var writer = new JsonTextWriter(streamWriter);
            
            _serializer.Serialize(writer, @object);
            
            writer.Flush();
            streamWriter.Flush();
        }
    }
}