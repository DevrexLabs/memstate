using System;
using Newtonsoft.Json;

namespace Memstate.JsonNet
{

    public class JsonInt32Converter : JsonConverter
    {

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //never called because CanWrite is false
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (reader.TokenType == JsonToken.Integer)
                ? Convert.ToInt32(reader.Value)
                : serializer.Deserialize(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }
    }
}