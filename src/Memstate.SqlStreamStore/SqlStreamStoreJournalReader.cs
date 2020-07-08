using System.Collections.Generic;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreJournalReader : JournalReader
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
        
        public override Task DisposeAsync() => Task.CompletedTask;

        public override IEnumerable<JournalRecord> ReadRecords(long fromRecord)
        {
            var pageSize = 200;

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