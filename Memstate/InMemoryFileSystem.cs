namespace Memstate
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// A virtual file system where each file is kept in memory as a MemoryStream.
    /// The intended use case is for internal testing 
    /// </summary>
    public class InMemoryFileSystem : IVirtualFileSystem
    {
        private readonly Dictionary<string, MemoryStream> _files 
            = new Dictionary<string, MemoryStream>();

        private readonly HashSet<string> _lockedFiles
            = new HashSet<string>();

        public bool Exists(string fileName) => _files.ContainsKey(fileName);

        public Stream OpenAppend(string path)
        {
            lock (_files)
            {
                var stream = OpenOrCreateStream(path);
                stream.Position = stream.Length;
                return stream;
            }
        }

        public Stream OpenRead(string path)
        {
            lock (_files)
            {
                if (!_files.ContainsKey(path))
                {
                    throw new FileNotFoundException(path);    
                }

                var result = OpenOrCreateStream(path);
                result.Position = 0;
                return result;
            }
        }

        private void ReleaseLock(string path)
        {
            _lockedFiles.Remove(path);
        }

        private MemoryStream OpenOrCreateStream(string path)
        {
            Ensure.That(() => !_lockedFiles.Contains(path), "Can't open locked file: " + path);
                
            if (!_files.ContainsKey(path))
            {
                _files[path] = new ReusableMemoryStream(() => ReleaseLock(path));
            }

            _lockedFiles.Add(path);
            return _files[path];
        }

        private class ReusableMemoryStream : MemoryStream
        {
            private readonly Action _onDispose;

            public ReusableMemoryStream(Action onDispose)
            {
                _onDispose = onDispose;
            }

            protected override void Dispose(bool disposing)
            {
                _onDispose.Invoke();
            }
        }
    }
}