using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreReader : IJournalReader
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly String _streamName;

        public EventStoreReader(IEventStoreConnection connection, ISerializer serializer, String streamName)
        {
            _connection = connection;
            _serializer = serializer;
            _streamName = streamName;
        }

        public void Dispose()
        {
        }

        public IEnumerable<JournalRecord> GetRecords()
        {
            long nextRecord = 0;
            var eventsPerSlice = 100;
            while (true)
            {
                var slice = _connection.ReadStreamEventsForwardAsync(_streamName, nextRecord, eventsPerSlice, false).Result;
                foreach (var @event in slice.Events.Select(e => e.Event))
                {
                    yield return @event.ToJournalRecord(_serializer);
                }
                if (slice.IsEndOfStream) break;
                nextRecord = slice.NextEventNumber;
            }
        }
    }
}