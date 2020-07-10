using System;
using System.IO;

namespace Memstate
{
    /// <summary>
    /// A read/write stream that allows reading forward and appending
    /// </summary>
    class DualStream : Stream
    {
        private readonly Stream _decoratedStream;
        private int _readPosition;

        public DualStream(Stream stream) => _decoratedStream = stream;

        public override void Flush()
        {
            lock(this) _decoratedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                _decoratedStream.Position = _readPosition;
                var bytesRead = _decoratedStream.Read(buffer, offset, count);
                _readPosition += bytesRead;
                return bytesRead;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                _decoratedStream.Position = _decoratedStream.Length;
                _decoratedStream.Write(buffer, offset, count);
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                lock (this) return _decoratedStream.Length;
            }
        } 
        
        public override long Position { get; set; }
    }
}