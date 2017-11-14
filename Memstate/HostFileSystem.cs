namespace Memstate
{
    using System.IO;

    internal class HostFileSystem : IVirtualFileSystem
    {
        public Stream OpenAppend(string path)
        {
            return File.Open(path, FileMode.Append, FileAccess.Write, FileShare.None);
        }

        public Stream OpenRead(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}