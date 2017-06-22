using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Core;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// IJournalWriter implementation that writes to an EventStore 3 instance
    /// </summary>
    public class EventStoreWriter  : IAccept<CommandChunk>, IDisposable
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly ISerializer _serializer;
        private readonly String _streamName;

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

        public async void Accept(CommandChunk chunk)
        {
            var id = Guid.NewGuid();
            var bytes = _serializer.Serialize(chunk);
            var eventData = new EventData(id, _streamName, false, bytes, null);
            //todo: add chunk meta data to event?
            var result = await _eventStore.AppendToStreamAsync(_streamName, ExpectedVersion.Any, eventData);
            //result.LogPosition.CommitPosition;

        }
    }
}