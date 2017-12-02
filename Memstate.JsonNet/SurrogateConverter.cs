using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace Memstate.JsonNet
{
    public class SurrogateConverter : JsonConverter
    {
        private readonly JsonSerializer _parent;

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="parent">TBD</param>
        public SurrogateConverter(JsonSerializer parent)
        {
            _parent = parent;
        }

        /// <summary>
        ///     Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(int) || objectType == typeof(float) || objectType == typeof(decimal))
                return true;

            if (objectType == typeof(object))
             return true;

            return false;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
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

        private object GetString(object value)
        {
            if (value is int)
                return "I" + ((int)value).ToString(NumberFormatInfo.InvariantInfo);
            if (value is float)
                return "F" + ((float)value).ToString(NumberFormatInfo.InvariantInfo);
            if (value is decimal)
                return "M" + ((decimal)value).ToString(NumberFormatInfo.InvariantInfo);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads the JSON representation of the object. This method is not hit.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }       
    }
}
