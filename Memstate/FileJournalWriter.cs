using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate
{
    public class FileJournalWriter : BatchingJournalWriter
    {
        private readonly int _pageSize;

        private readonly ISerializer _serializer;

        private readonly MemstateSettings _settings;

        private readonly string _fileName;

        private readonly bool _staticFileName;
        
        private ReusableStream _journalStream;

        /// <summary>
        /// Id of the next record to be written
        /// </summary>
        private long _nextRecord;

        private int _writtenRecords;
        
        private int _fileNameIndex;

        public FileJournalWriter(MemstateSettings settings, string fileName, long nextRecord, int pageSize)
            : base(settings)
        {
            _pageSize = pageSize;
            _nextRecord = nextRecord;
            _serializer = settings.CreateSerializer();
            _writtenRecords = (int) (nextRecord % _pageSize);
            _settings = settings;
            _fileName = fileName;

            if (_fileName.Contains("{0}"))
            {
                var i = 0;

                while (settings.FileSystem.Exists(string.Format(_fileName, i++)))
                {
                }

                _fileNameIndex = i;
            }
            else
            {
                _staticFileName = true;
            }
        }

        public event RecordsWrittenHandler RecordsWritten = delegate { };

        public long NextRecord => _nextRecord;

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);

            if (_journalStream != null)
            {
                await _journalStream.FlushAsync().ConfigureAwait(false);

                _journalStream.Dispose();
            }
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            var records = commands.Select(ToJournalRecord).ToArray();

            foreach (var chunk in records.Split(_pageSize - _writtenRecords, _pageSize))
            {
                using (var stream = Open(_writtenRecords + chunk.Length < _pageSize))
                {
                    _serializer.WriteObject(stream, chunk);

                    _writtenRecords += chunk.Length;

                    if (_writtenRecords >= _pageSize)
                    {
                        _writtenRecords = 0;
                    }
                }
            }

            RecordsWritten.Invoke(records);
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }

        private Stream Open(bool reuse)
        {
            var fileName = CreateFileName();

            if (_journalStream == null || _journalStream.Disposed)
            {
                _journalStream = new ReusableStream(_settings.FileSystem.OpenAppend(fileName), reuse || _staticFileName);
            }

            return _journalStream;
        }

        private string CreateFileName()
        {
            return _staticFileName ? _fileName : string.Format(_fileName, _fileNameIndex++);
        }

        private class ReusableStream : Stream
        {
            private readonly Stream _innerStream;

            private readonly bool _reuse;

            public ReusableStream(Stream innerStream, bool reuse)
            {
                _innerStream = innerStream;
                _reuse = reuse;
            }

            public override bool CanRead => _innerStream.CanRead;

            public override bool CanSeek => _innerStream.CanSeek;

            public override bool CanWrite => _innerStream.CanWrite;

            public override long Length => _innerStream.Length;

            public bool Disposed { get; private set; }

            public override long Position
            {
                get => _innerStream.Position;
                set => _innerStream.Position = value;
            }

            public override void Flush()
            {
                _innerStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _innerStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _innerStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _innerStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _innerStream.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
                if (_reuse)
                {
                    return;
                }

                Disposed = true;

                base.Dispose(disposing);
            }
        }
    }
}