using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Core;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreWriter : BatchingJournalWriter
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly ISerializer _serializer;
        private readonly string _streamName;

        private readonly ILogger _logger;

        public EventStoreWriter(Config config, IEventStoreConnection connection, ISerializer serializer, String streamName) 
            : base(config)
        {
            _logger = config.CreateLogger<EventStoreWriter>();
            _serializer = serializer;
            _eventStore = connection;
            _streamName = streamName;
        }

        protected override async Task OnCommandBatch(IEnumerable<Command> commands)
        {
            var events = commands.Select(ToEventData);
            _logger.LogDebug("Writing {0} events", commands.Count());
            var writeResult = await _eventStore.AppendToStreamAsync(_streamName, ExpectedVersion.Any, events);
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