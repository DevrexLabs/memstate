using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Configuration;
using Memstate.Logging;


namespace Memstate.EventStore
{
    public class EventStoreReader : IJournalReader
    {
        private readonly IEventStoreConnection _connection;

        private readonly ISerializer _serializer;

        private readonly string _streamName;

        private readonly int _eventsPerSlice;

        private readonly ILog _logger;

        public EventStoreReader(IEventStoreConnection connection)
        {
            var config = Config.Current;
            var settings = config.GetSettings<EngineSettings>();
            var eventStoreSettings = config.GetSettings<EventStoreSettings>();
            _logger = LogProvider.GetCurrentClassLogger();
            _connection = connection;
            _serializer = config.Serializers.Resolve(eventStoreSettings.SerializerName);
            _streamName = eventStoreSettings.StreamName;
            _eventsPerSlice = eventStoreSettings.EventsPerSlice;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            var nextRecord = fromRecord;

            _logger.Info("GetRecords from {0}", nextRecord);

            while (true)
            {
                var slice = _connection.ReadStreamEventsForwardAsync(_streamName, nextRecord, _eventsPerSlice, false).Result;

                _logger.Debug("{0} events in slice from {0}", slice.Events.Length, slice.FromEventNumber);

                foreach (var @event in slice.Events.Select(e => e.Event))
                {
                    yield return @event.ToJournalRecord(_serializer);
                }

                if (slice.IsEndOfStream)
                {
                    break;
                }

                nextRecord = slice.NextEventNumber;
            }

            _logger.Info("GetRecords completed");
        }
    }
}