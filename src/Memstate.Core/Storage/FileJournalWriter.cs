using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileJournalWriter : BatchingJournalWriter
    {
        private readonly Stream _journalStream;

        private readonly ISerializer _serializer;

        /// <summary>
        /// Id of the next record to be written
        /// </summary>
        private long _nextRecord;

        public FileJournalWriter(Config config,  string fileName, long nextRecord)
            :base(config.GetSettings<EngineSettings>())
        {
            _nextRecord = nextRecord;
            _journalStream = config.FileSystem.OpenAppend(fileName);
            _serializer = config.CreateSerializer();
        }

        public event RecordsWrittenHandler RecordsWritten = delegate { };

        public long NextRecord => _nextRecord;

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync().NotOnCapturedContext();

            await _journalStream.FlushAsync().NotOnCapturedContext();

            _journalStream.Dispose();
        }

        protected override Task OnCommandBatch(IEnumerable<Command> commands)
        {
            var records = commands.Select(ToJournalRecord).ToArray();

            _serializer.WriteObject(_journalStream, records);
            _journalStream.Flush();

            RecordsWritten.Invoke(records);
            return Task.CompletedTask;
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }
    }
}