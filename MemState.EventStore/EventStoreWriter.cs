using System;
using System.Linq;
using System.Net;
using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
    public class EventStoreWriter : IHandle<CommandChunk>, IDisposable
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

        public void Dispose()
        {
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

        public async void Handle(CommandChunk chunk)
        {
            var events = chunk.Commands.Select(ToEventData);
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