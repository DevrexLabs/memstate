using EventStore.ClientAPI;
using Memstate.Core;

namespace Memstate.EventStore
{
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
}