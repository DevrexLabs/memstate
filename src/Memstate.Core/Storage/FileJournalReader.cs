using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileJournalReader : JournalReader
    {
        private readonly string _fileName;
        private readonly ISerializer _serializer;
        private readonly FileJournalWriter _writer;
        private readonly IFileSystem _fileSystem;

        public FileJournalReader(Config config, string fileName, FileJournalWriter writer)
        {
            _writer = writer;
            _fileName = fileName;
            _serializer = config.CreateSerializer();
            _fileSystem = config.FileSystem;
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        public override Task Subscribe(long first, long last, Action<JournalRecord> recordHandler, CancellationToken cancellationToken)
        {
            long lastRecordNumber = 0;
            
            return Task.Run(() =>
            {
                foreach (var record in ReadRecords(first))
                {
                    if (record.RecordNumber >= first && record.RecordNumber <= last)
                        recordHandler.Invoke(record);
                    if (record.RecordNumber >= last) return;
                    lastRecordNumber = record.RecordNumber;
                }

                var buffer = new BlockingCollection<JournalRecord>();
                _writer.RecordsWritten += records =>
                {
                    foreach (var record in records) buffer.Add(record, cancellationToken);
                };

                _writer.Notify(lastRecordNumber + 1);
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (buffer.TryTake(out var record, 1000))
                    {
                        if (record.RecordNumber >= first && record.RecordNumber <= last)
                            recordHandler.Invoke(record);
                        if (record.RecordNumber >= last) break;
                    }
                }
            }, cancellationToken);
        }

        public override IEnumerable<JournalRecord> ReadRecords(long from)
        {
            if (!_fileSystem.Exists(_fileName)) yield break;

            using (var stream = _fileSystem.OpenRead(_fileName))
            {
                foreach (var record in _serializer.ReadObjects<JournalRecord>(stream))
                {
                    yield return record;
                }
            }
        }
    }
}