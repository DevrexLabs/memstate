﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Memstate.JsonNet
{
    public class JsonSerializerAdapter : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonSerializerAdapter(MemstateSettings config = null)
        {

            IList<JsonConverter> converters = new List<JsonConverter>();
            converters.Add(new SurrogateConverter(_serializer));


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
            //SurrogateConverter Changes.
            if (result != null && result.ToString().Contains("$"))
            {
                var output = result as Newtonsoft.Json.Linq.JObject;
                if (output["$"] != null)
                {
                    var convertedoutput = SurrogateConverter.GetValue(output["$"].ToString());
                    return convertedoutput;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return result;
            }
        }

        public IEnumerable<T> ReadObjects<T>(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(streamReader);
            jsonReader.SupportMultipleContent = true;
            while (jsonReader.Read())
            {
                yield return _serializer.Deserialize<T>(jsonReader);
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