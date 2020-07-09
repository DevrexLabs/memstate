using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Configuration;
using Memstate.Logging;

namespace Memstate.EventStore
{
    public class EventStoreWriter : BatchingJournalWriter
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;
        private readonly ILog _logger;

        public EventStoreWriter(Config config, IEventStoreConnection connection)
            :base(config.GetSettings<EngineSettings>())
        {
            var settings = config.GetSettings<EventStoreSettings>();
            _connection = connection;
            _logger = LogProvider.GetCurrentClassLogger();
            _serializer = config.CreateSerializer(settings.SerializerName);
            _streamName = settings.StreamName 
                          ?? config.GetSettings<EngineSettings>().StreamName;
        }

        protected override async Task OnCommandBatch(IEnumerable<Command> commands)
        {
            var events = commands.Select(ToEventData).ToArray();
            _logger.Debug("Writing {0} events", events.Length);
            var writeResult = await _connection.AppendToStreamAsync(_streamName, ExpectedVersion.Any, events);
            _logger.Debug("Write async completed, lastRecord: {0}", writeResult.NextExpectedVersion);
        }

        private EventData ToEventData(Command cmd)
        {
            var typeName = cmd.GetType().ToString();
            var bytes = _serializer.Serialize(cmd);

            return new EventData(
                eventId: cmd.CommandId,
                type: typeName,
                isJson: false,
                data: bytes,
                metadata: null);
        }
    }
}