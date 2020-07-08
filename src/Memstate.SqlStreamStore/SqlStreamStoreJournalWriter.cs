using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreJournalWriter : BatchingJournalWriter
    {
        private readonly IStreamStore _streamStore;
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;

        public SqlStreamStoreJournalWriter(Config config, IStreamStore streamStore, StreamId streamId)
        :base(config.GetSettings<EngineSettings>())
        {
            _streamStore = streamStore;
            _streamId = streamId;
            _serializer = config.CreateSerializer();
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