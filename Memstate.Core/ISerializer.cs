using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Memstate.Core
{
    public interface ISerializer
    {
        void WriteObject(Stream stream, object @object);
        object ReadObject(Stream stream);

        IEnumerable<T> ReadObjects<T>(Stream stream);
    }
}