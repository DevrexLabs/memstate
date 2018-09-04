using System.IO;

namespace Memstate
{
    public interface IFileSystem
    {
        Stream OpenAppend(string path);

        Stream OpenRead(string path);

        bool Exists(string fileName);
    }
}