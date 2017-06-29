using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memstate.Core
{
    public class FileJournalWriter : IHandle<Command>
    {
        private long _nextRecord;
        private readonly Stream _journalStream;
        private readonly Batcher<Command> _commandBatcher;
        private readonly ISerializer _serializer;

        public event RecordsWrittenHandler RecordsWritten = delegate { };

        public FileJournalWriter(ISerializer serializer, string fileName, long nextRecord)
        {
            _nextRecord = nextRecord;
            _journalStream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.None);
            _commandBatcher = new Batcher<Command>(OnCommandBatch, 100);
            _serializer = serializer;
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }

        private void OnCommandBatch(IEnumerable<Command> commands)
        {
            var records = commands.Select(ToJournalRecord).ToArray();
            _serializer.WriteObject(_journalStream, records);
            RecordsWritten.Invoke(records);
        }

        public void Handle(Command command)
        {
            _commandBatcher.Append(command);
        }

        public void Dispose()
        {
            _commandBatcher.Dispose();
            _journalStream.Flush();
            _journalStream.Dispose();
        }
    }
}