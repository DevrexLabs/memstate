using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreEngineBuilder : IEngineBuilder
    {
        private readonly Config _config;
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;

        private readonly ILogger _logger;

        public EventStoreEngineBuilder(Config config, IEventStoreConnection connection, ISerializer serializer, string streamName)
        {
            _logger = config.CreateLogger<EventStoreEngineBuilder>();
            _config = config;
            _connection = connection;
            _serializer = serializer;
            _streamName = streamName;
        }
        
        public Engine<T> Load<T>() where T : class, new()
        {
            _logger.LogInformation("Loading Engine from stream {0}", _streamName);
            var reader = new EventStoreReader(_config, _connection, _serializer, _streamName);
            var loader = new ModelLoader();
            var model = loader.Load<T>(reader);
            _logger.LogInformation("Model loaded, LastRecordNumber {0}", loader.LastRecordNumber);

            var subscriptionSource = new EventStoreSubscriptionSource(_config, _connection, _serializer, _streamName);
            var writer = new EventStoreWriter(_config, _connection, _serializer, _streamName);
            return new Engine<T>(_config, model,subscriptionSource,writer, loader.LastRecordNumber + 1);
        }
    }
}