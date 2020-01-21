using System;
using System.Collections.Generic;
using System.IO;

namespace Memstate
{
    public interface ISerializer
    {
        void WriteObject(Stream stream, object @object);

        object ReadObject(Stream stream);

        IEnumerable<T> ReadObjects<T>(Stream stream);

        /// <summary>
        /// Serialize to a string
        /// </summary>
        string ToString(object @object);

        /// <summary>
        /// Deserialize from string
        /// </summary>
        object FromString(String s);
    }
}