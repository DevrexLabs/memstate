using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreJournalWriter : BatchingJournalWriter
    {
        private readonly IStreamStore _streamStore;
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;

        public SqlStreamStoreJournalWriter(IStreamStore streamStore, StreamId streamId, ISerializer serializer)
        {
            _streamStore = streamStore;
            _streamId = streamId;
            _serializer = serializer;
        }

        protected override Task OnCommandBatch(IEnumerable<Command> commands)
        {
            var messages = commands.Select(ToNewStreamMessage).ToArray();
            return _streamStore.AppendToStream(_streamId, ExpectedVersion.Any, messages );
        }

        private NewStreamMessage ToNewStreamMessage(Command command)
        {
            var commandAsString = _serializer.ToString(command);
            var id = command.CommandId;
            return new NewStreamMessage(id, command.GetType().Name, commandAsString);
        }
    }
}