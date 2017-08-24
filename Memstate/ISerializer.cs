using System.Collections.Generic;
using System.IO;

namespace Memstate
{
    public interface ISerializer
    {
        void WriteObject(Stream stream, object @object);
        object ReadObject(Stream stream);

        IEnumerable<T> ReadObjects<T>(Stream stream);
    }
}