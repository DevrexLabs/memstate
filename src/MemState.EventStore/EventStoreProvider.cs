using System.Threading.Tasks;

using EventStore.ClientAPI;
using Memstate.Configuration;

namespace Memstate.EventStore
{
    public class EventStoreProvider : IStorageProvider, IAsyncDisposable
    {
        private readonly IEventStoreConnection _connection;
        private readonly Config _config;

        public EventStoreProvider(Config config)
        {
            _config = config;
            var eventStoreSettings = config.GetSettings<EventStoreSettings>();
            _connection = EventStoreConnection.Create(eventStoreSettings.ConnectionString);
        }

        public Task Provision()
        {
            return _connection.ConnectAsync();
        }

        public IJournalReader CreateJournalReader()
        {
            return new EventStoreReader(_config, _connection);
        }

        public IJournalWriter CreateJournalWriter()
        {
            return new EventStoreWriter(_config, _connection);
        }

        public Task DisposeAsync()
        {
            _connection.Close();
            return Task.CompletedTask;
        }
    }
}