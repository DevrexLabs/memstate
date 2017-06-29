using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memstate.Core
{
    public class FileJournalReader 
    {
        private readonly FileStream _journalStream;
        private readonly ISerializer _serializer;

        public FileJournalReader(String fileName, ISerializer serializer)
        {
            _journalStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            _serializer = serializer;
        }

        public void Dispose()
        {
            _journalStream.Dispose();
        }

        public IEnumerable<JournalRecord> GetRecords()
        {
            foreach (var records in _serializer.ReadObjects<JournalRecord[]>(_journalStream))
            {
                foreach (var record in records)
                {
                    yield return record;
                }
            }
        }
    }
}