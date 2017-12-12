using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreWriter : BatchingJournalWriter
    {
        private readonly IEventStoreConnection _connection;

        private readonly ISerializer _serializer;

        private readonly string _streamName;

        private readonly ILogger _logger;

        public EventStoreWriter(MemstateSettings settings, IEventStoreConnection connection)
            : base(settings)
        {
            _connection = connection;
            _logger = settings.CreateLogger<EventStoreWriter>();

            var eventStoreSettings = new EventStoreSettings(settings);

            _serializer = eventStoreSettings.CreateSerializer();
            _streamName = eventStoreSettings.StreamName;
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            var events = commands.Select(ToEventData).ToArray();

            _logger.LogDebug("Writing {0} events", events.Length);

            var writeResult = _connection.AppendToStreamAsync(_streamName, ExpectedVersion.Any, events).Result;

            _logger.LogDebug("Write async completed, lastRecord: {0}", writeResult.NextExpectedVersion);
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