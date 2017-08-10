using EventStore.ClientAPI;
using Memstate.Core;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Memstate.EventStore
{
    public class EventStoreEngineBuilder : IEngineBuilder
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly string _streamName;

        private ILogger _logger = Logging.CreateLogger<EventStoreEngineBuilder>();

        public EventStoreEngineBuilder(IEventStoreConnection connection, ISerializer serializer, string streamName)
        {
            _connection = connection;
            _serializer = serializer;
            _streamName = streamName;
        }
        
        public Engine<T> Load<T>() where T : class, new()
        {
            _connection.Closed += (s, e) => _logger.LogInformation("ES connection {0} closed, reason: {1}", e.Connection.ConnectionName, e.Reason);
            _connection.Disconnected += (s, e) => _logger.LogWarning("ES disconnected, {0}", e.Connection.ConnectionName);
            _connection.ErrorOccurred += (s, e) => _logger.LogError("ES connection {0} error: ", default(EventId), e.Exception, e.Connection.ConnectionName);
            _connection.Reconnecting += (s, e) => _logger.LogInformation("ES {0} reconnecting", e.Connection.ConnectionName);
            _connection.Connected += (s, e) => _logger.LogInformation("ES {0} connected", e.Connection.ConnectionName);

            _logger.LogInformation("Loading Engine from stream {0}", _streamName);
            var reader = new EventStoreReader(_connection, _serializer, _streamName);
            var loader = new ModelLoader();
            var model = loader.Load<T>(reader);
            _logger.LogInformation("Model loaded, LastRecordNumber {0}", loader.LastRecordNumber);

            var subscriptionSource = new EventStoreSubscriptionSource(_connection, _serializer, _streamName);
            var writer = new EventStoreWriter(_connection, _serializer, _streamName);
            return new Engine<T>(model,subscriptionSource,writer, loader.LastRecordNumber + 1);
        }
    }
}