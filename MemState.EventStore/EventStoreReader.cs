using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreReader : IJournalReader
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;
        private readonly int _eventsPerSlice;

        private readonly ILogger _logger;


        public EventStoreReader(MemstateSettings config, EventStoreSettings eventStoreSettings, IEventStoreConnection connection)
        {
            _logger = config.CreateLogger<EventStoreReader>();
            _connection = connection;
            _serializer = config.CreateSerializer();
            _streamName = eventStoreSettings.StreamName;
            _eventsPerSlice = eventStoreSettings.EventsPerSlice;
        }

        public void Dispose()
        {
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            long nextRecord = fromRecord;
            _logger.LogInformation("GetRecords from {0}", nextRecord);
            while (true)
            {
                var slice = _connection.ReadStreamEventsForwardAsync(_streamName, nextRecord, _eventsPerSlice, false).Result;
                _logger.LogDebug("{0} events in slice from {0}", slice.Events.Length, slice.FromEventNumber);
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

            _logger.LogInformation("GetRecords completed");
        }
    }
}