using System;
using System.Collections.Generic;
using System.IO;

namespace Memstate
{
    /// <summary>
    /// A file system where each file is kept in memory as a MemoryStream.
    /// Useful as a mock file system for integration testing.
    /// Faster than file i/o and no left over files in the file system
    /// </summary>
    public class InMemoryFileSystem : IVirtualFileSystem
    {
        private readonly Dictionary<string, MemoryStream> _files = new Dictionary<string, MemoryStream>();

        private readonly HashSet<string> _lockedFiles = new HashSet<string>();

        // NOTE: Should this property use a lock?
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
            if (_lockedFiles.Contains(path)) throw new IOException("File is locked");

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