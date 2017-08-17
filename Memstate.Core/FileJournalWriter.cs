using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class FileJournalWriter : BatchingJournalWriter
    {
        private long _nextRecord;
        private readonly Stream _journalStream;
        private readonly ISerializer _serializer;

        public event RecordsWrittenHandler RecordsWritten = delegate { };

        public FileJournalWriter(Config config, ISerializer serializer, string fileName, long nextRecord) 
            : base(config)
        {
            _nextRecord = nextRecord;
            _journalStream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.None);
            _serializer = serializer;
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            var records = commands.Select(ToJournalRecord).ToArray();
            _serializer.WriteObject(_journalStream, records);
            RecordsWritten.Invoke(records);
        }

        public override void Dispose()
        {
            base.Dispose();
            _journalStream.Flush();
            _journalStream.Dispose();
        }
    }
}