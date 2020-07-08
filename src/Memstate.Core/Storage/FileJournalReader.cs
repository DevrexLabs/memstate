using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileJournalReader : JournalReader
    {
        private readonly Stream _journalStream;

        private readonly ISerializer _serializer;

        public FileJournalReader(string fileName)
        {
            var cfg = Config.Current;
            var settings = cfg.GetSettings<EngineSettings>();
            _journalStream = cfg.FileSystem.OpenRead(fileName);
            _serializer = cfg.CreateSerializer();
        }

        public override Task DisposeAsync()
        {
            return Task.Run((Action) _journalStream.Dispose);
        }

        public override IEnumerable<JournalRecord> ReadRecords(long from)
        {
            throw new NotImplementedException();
        }
    }
}