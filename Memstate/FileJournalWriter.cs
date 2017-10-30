namespace Memstate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class FileJournalWriter : BatchingJournalWriter
    {
        private readonly Stream _journalStream;
        private readonly ISerializer _serializer;
        private long _nextRecord;


        public FileJournalWriter(Settings settings, string fileName, long nextRecord) 
            : base(settings)
        {
            _nextRecord = nextRecord;
            _journalStream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.None);
            _serializer = settings.CreateSerializer();
        }

        public event RecordsWrittenHandler RecordsWritten = delegate { };

        public override void Dispose()
        {
            base.Dispose();
            _journalStream.Flush();
            _journalStream.Dispose();
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            var records = commands.Select(ToJournalRecord).ToArray();
            _serializer.WriteObject(_journalStream, records);
            RecordsWritten.Invoke(records);
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }
    }
}