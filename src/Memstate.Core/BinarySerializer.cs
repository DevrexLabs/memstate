using System;
using System.Collections.Generic;
using System.IO;

namespace Memstate
{
    public abstract class BinarySerializer : ISerializer
    {
        public abstract object ReadObject(Stream stream);

        public abstract void WriteObject(Stream stream, object @object);


        public IEnumerable<T> ReadObjects<T>(Stream stream)
        {
            while (stream.Position < stream.Length - 1)
            {
                yield return (T)ReadObject(stream);
            }
        }

        public string ToString(object @object)
        {
            var bytes = this.Serialize(@object);
            return Convert.ToBase64String(bytes);
        }

        public object FromString(string s)
        {
            var bytes = Convert.FromBase64String(s);
            return this.Deserialize(bytes);
        }
    }
}