using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Memstate
{
    public class FileJournalReader : IJournalReader
    {
        private readonly Stream _journalStream;

        private readonly ISerializer _serializer;

        public FileJournalReader(string fileName, MemstateSettings settings)
        {
            _journalStream = settings.FileSystem.OpenRead(fileName);
            _serializer = settings.CreateSerializer();
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