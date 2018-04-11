using System;
using Newtonsoft.Json;
using System.Globalization;

namespace Memstate.JsonNet
{
    public class SurrogateConverter : JsonConverter
    {
        public SurrogateConverter(JsonSerializer parent)
        {
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(int) || objectType == typeof(float) || objectType == typeof(decimal))
            {
                return true;
            }

            return objectType == typeof(object);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is int || value is decimal || value is float)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$");
                writer.WriteValue(GetString(value));
                writer.WriteEndObject();
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private static object GetString(object value)
        {
            switch (value)
            {
                case int _:
                    return "I" + ((int) value).ToString(NumberFormatInfo.InvariantInfo);
                case float _:
                    return "F" + ((float) value).ToString(NumberFormatInfo.InvariantInfo);
                case decimal _:
                    return "M" + ((decimal) value).ToString(NumberFormatInfo.InvariantInfo);
            }

            throw new NotSupportedException();
        }
    }
}