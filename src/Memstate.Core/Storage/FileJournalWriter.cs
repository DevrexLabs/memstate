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
        /// <summary>
        /// The stream to write journal records to
        /// </summary>
        private Stream _journalStream;

        /// <summary>
        /// Serializer used to serialize journal records
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Id of the next record to be written
        /// </summary>
        private long _nextRecord;

        private readonly Config _config;

        
        private readonly string _fileName;

        public FileJournalWriter(Config config,  string fileName)
            :base(config.GetSettings<EngineSettings>())
        {
            _nextRecord = -1;
            _serializer = config.CreateSerializer();
            _config = config;
            _fileName = fileName;
        }

        public event RecordsWrittenHandler RecordsWritten = delegate { };

        public long NextRecord => _nextRecord;

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync().NotOnCapturedContext();
            await _journalStream.FlushAsync().NotOnCapturedContext();
            _journalStream.Dispose();
        }


        private readonly List<Command> _buffer = new List<Command>(4096);

        protected override async Task OnCommandBatch(IEnumerable<Command> commands)
        {
        
            _buffer.AddRange(commands);
            if (_journalStream is null) return;
            
            var records = _buffer.Select(ToJournalRecord).ToArray();
            _buffer.Clear();

            foreach(var record in records)
                _serializer.WriteObject(_journalStream, record);

            await _journalStream.FlushAsync().NotOnCapturedContext();
            
            RecordsWritten.Invoke(records);
        }
        
        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }

        /// <summary>
        /// Notify that loading is complete and writing to
        /// the underlying file can commence
        /// </summary>
        internal Task StartWritingFrom(long nextRecordNumber)
        {
            _nextRecord = nextRecordNumber;
            _journalStream = _config.FileSystem.OpenAppend(_fileName);
            return OnCommandBatch(Array.Empty<Command>());
        }
    }
}