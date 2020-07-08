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

        public FileJournalReader(string fileName, FileJournalWriter writer)
        {
            _writer = writer;
            _fileName = fileName;
            var config = Config.Current;
            _serializer = config.CreateSerializer();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        public override Task Subscribe(long first, long last, Action<JournalRecord> recordHandler, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var buffer = new BlockingCollection<JournalRecord>();
                _writer.RecordsWritten += records =>
                {
                    foreach (var record in records) buffer.Add(record, cancellationToken);
                };

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
            var fs = Config.Current.FileSystem;
            using (var stream = fs.OpenRead(_fileName))
            {
                foreach (var record in _serializer.ReadObjects<JournalRecord>(stream))
                {
                    yield return record;
                }
            }
        }
    }
}