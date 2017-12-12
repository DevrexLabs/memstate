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
            var converters = new List<JsonConverter>
            {
                new SurrogateConverter(_serializer)
            };

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
                CheckAdditionalContent = false,
                Converters = converters
            };

            _serializer = JsonSerializer.Create(settings);
        }

        public object ReadObject(Stream serializationStream)
        {
            var streamReader = new StreamReader(serializationStream);
            var line = streamReader.ReadLine();
            var reader = new JsonTextReader(new StringReader(line));
            var result = _serializer.Deserialize(reader);
            var output = result as Newtonsoft.Json.Linq.JObject;

            if (output?["$"] == null)
            {
                return result;
            }

            var convertedoutput = GetValue(output["$"].ToString());

            return convertedoutput;
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

        public object GetValue(string input)
        {
            var type = input.Substring(0, 1);
            var value = input.Substring(1);
            
            switch (type)
            {
                case "I":
                    return int.Parse(value, NumberFormatInfo.InvariantInfo);
                case "F":
                    return float.Parse(value, NumberFormatInfo.InvariantInfo);
                case "M":
                    return decimal.Parse(value, NumberFormatInfo.InvariantInfo);
            }

            throw new NotSupportedException();
        }
    }
}