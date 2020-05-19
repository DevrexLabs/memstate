using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreJournalReader : IJournalReader
    {
        private readonly IStreamStore _streamStore;
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;
        
        public SqlStreamStoreJournalReader(
            IStreamStore streamStore, 
            StreamId streamId,
            ISerializer serializer)
        {
            _streamStore = streamStore;
            _streamId = streamId;
            _serializer = serializer;
        }
        
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            var pageSize = 20;

            while (true)
            {
                var page = _streamStore.ReadStreamForwards(
                    _streamId, (int) fromRecord, pageSize).Result;
                foreach (var message in page.Messages)
                    yield return RecordFromStreamMessage(message);
                if (page.IsEnd) break;
                fromRecord += page.Messages.Length;
            }
        }

        private JournalRecord RecordFromStreamMessage(StreamMessage streamMessage)
        {
            var commandString = streamMessage.GetJsonData().Result;
            var command = (Command) _serializer.FromString(commandString);
            return new JournalRecord(
                streamMessage.StreamVersion, 
                streamMessage.CreatedUtc,
                command);
        }
    }
}