using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreWriter : BatchingJournalWriter
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly ISerializer _serializer;
        private readonly string _streamName;

        public EventStoreWriter(IEventStoreConnection connection, ISerializer serializer, String streamName)
        {
            _serializer = serializer;
            _eventStore = connection;
            _streamName = streamName;
        }

        protected override async Task OnCommandBatch(IEnumerable<Command> commands)
        {
            var events = commands.Select(ToEventData);
            var writeResult = await _eventStore.AppendToStreamAsync(_streamName, ExpectedVersion.Any, events);
        }

        private EventData ToEventData(Command cmd)
        {
            var typeName = cmd.GetType().ToString();
            var bytes = _serializer.Serialize(cmd);
            return new EventData(
                eventId: cmd.Id,
                type: typeName,
                isJson: false,
                data: bytes,
                metadata: null);
        }
    }
}