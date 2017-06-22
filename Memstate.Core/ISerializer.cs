using System.IO;

namespace Memstate.Core
{
    public interface ISerializer
    {
        void Serialize(Stream stream, object graph);
        object Deserialize(Stream stream);
    }
}