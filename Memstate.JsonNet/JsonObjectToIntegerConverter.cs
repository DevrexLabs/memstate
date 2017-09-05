using System;
using Newtonsoft.Json;

namespace Memstate.JsonNet
{
    //unused leftover, u
    internal class Boxed<T>
    {
        public T Value { get; set; }

        public Boxed(T value)
        {
            Value = value;
        }
    }

    public class JsonObjectToIntegerConverter : JsonConverter
    {
        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Int32 int32) serializer.Serialize(writer, new Boxed<int>(int32));
            else serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                long val = Convert.ToInt64(reader.Value);
                if (val <= Byte.MaxValue && val >= Byte.MinValue) return (byte) val;
                if (val <= short.MaxValue && val >= short.MinValue) return (short) val;
                if (val <= int.MaxValue && val >= int.MinValue) return (int) val;
                return val;
            }
            return serializer.Deserialize(reader);
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object) ||
                   objectType == typeof(Int32) ||
                   objectType == typeof(Boxed<int>);
        }
    }
}