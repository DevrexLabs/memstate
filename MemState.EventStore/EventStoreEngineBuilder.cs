using EventStore.ClientAPI;

namespace Memstate.EventStore
{
    using Microsoft.Extensions.Logging;

    public class EventStoreEngineBuilder : IEngineBuilder
    {
        private readonly Settings _config;
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;

        private readonly ILogger<EventStoreEngineBuilder> _logger;

        public EventStoreEngineBuilder(Settings config, IEventStoreConnection connection = null)
        {
            _logger = config.CreateLogger<EventStoreEngineBuilder>();
            _config = config;
            _connection = connection ?? ConnectToLocalhost();
            _serializer = config.GetSerializer();
            _streamName = config.StreamName;
        }

        public Engine<T> Build<T>() where T : class, new()
        {
            return Build(new T());
        }

        public Engine<T> Build<T>(T initialModel) where T : class
        {
            _logger.LogInformation("Loading Engine from stream {0}", _streamName);
            var reader = new EventStoreReader(_config, _connection, _serializer, _streamName);
            var loader = new ModelLoader();
            var model = loader.Load(reader, initialModel);
            _logger.LogInformation("Model loaded, LastRecordNumber {0}", loader.LastRecordNumber);

            var subscriptionSource = new EventStoreSubscriptionSource(_config, _connection, _serializer, _streamName);
            var writer = new EventStoreWriter(_config, _connection, _serializer, _streamName);
            return new Engine<T>(_config, model,subscriptionSource,writer, loader.LastRecordNumber + 1);
        }

        private IEventStoreConnection ConnectToLocalhost()
        {
            const string DefaultConnectionString = "ConnectTo=tcp://admin:changeit@localhost:1113";
            var connection = EventStoreConnection.Create(DefaultConnectionString);
            connection.ConnectAsync().Wait();
            return connection;
        }
    }
}