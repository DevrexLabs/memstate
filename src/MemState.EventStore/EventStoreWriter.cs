using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using Memstate.Logging;

namespace Memstate.EventStore
{
    public class EventStoreWriter : BatchingJournalWriter
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;
        private readonly ILog _logger;

        public EventStoreWriter(EventStoreSettings settings, IEventStoreConnection connection)
        {
            _connection = connection;
            _logger = LogProvider.GetCurrentClassLogger();
            _serializer = settings.CreateSerializer();
            _streamName = settings.StreamName;
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            var events = commands.Select(ToEventData).ToArray();
            _logger.Debug("Writing {0} events", events.Length);
            var writeResult = _connection.AppendToStreamAsync(_streamName, ExpectedVersion.Any, events).Result;
            _logger.Debug("Write async completed, lastRecord: {0}", writeResult.NextExpectedVersion);
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