using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileJournalReader : IJournalReader
    {
        private readonly Stream _journalStream;

        private readonly ISerializer _serializer;

        public FileJournalReader(string fileName)
        {
            var cfg = Config.Current;
            var settings = cfg.Resolve<MemstateSettings>();
            _journalStream = cfg.FileSystem.OpenRead(fileName);
            _serializer = cfg.CreateSerializer();
        }

        public Task DisposeAsync()
        {
            return Task.Run((Action) _journalStream.Dispose);
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            foreach (var record in _serializer.ReadObjects<JournalRecord>(_journalStream))
            {
                if (record.RecordNumber >= fromRecord)
                {
                    yield return record;
                }
            }
        }
    }
}