using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public static class EventStoreExtensions
    {
        public static JournalRecord ToJournalRecord(this RecordedEvent @event, ISerializer serializer)
        {
            var command = (Command) serializer.Deserialize(@event.Data);
            return new JournalRecord(@event.EventNumber, @event.Created, command);
        }
    }
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
            _connection.Dispose();    
        }

        public IEnumerable<JournalRecord> GetRecords()
        {
            long nextRecord = 1;
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

    public class EventStoreEngineBuilder : IEngineBuilder
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;

        public EventStoreEngineBuilder(IEventStoreConnection connection, ISerializer serializer, string streamName)
        {
            _connection = connection;
            _serializer = serializer;
            _streamName = streamName;
        }
        
        public Engine<T> Load<T>() where T : class, new()
        {
            var reader = new EventStoreReader(_connection, _serializer, _streamName);
            var loader = new ModelLoader();
            var model = loader.Load<T>(reader);
            var subscriptionSource = new EventStoreSubscriptionSource(_connection, _serializer, _streamName);
            var writer = new EventStoreWriter(_connection, _serializer, _streamName);
            return new Engine<T>(model,subscriptionSource,writer, loader.LastRecordNumber + 1);
        }
    }

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

        public  override void Dispose()
        {
            base.Dispose();
            _eventStore.Close();
        }

        /// <summary>
        /// create an instance with an open connection to an event store instance
        /// </summary>
        /// <returns></returns>
        public static EventStoreWriter Create(ISerializer serializer, string streamName)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113);
            var connection = EventStoreConnection.Create(endPoint);
            connection.ConnectAsync().Wait();

            return new EventStoreWriter(connection, serializer, streamName);
        }

        protected override async void OnCommandBatch(IEnumerable<Command> commands)
        {
            var events = commands.Select(ToEventData);
            await _eventStore.AppendToStreamAsync(_streamName, ExpectedVersion.Any, events);
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