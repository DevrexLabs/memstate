using System;
using System.Threading.Tasks;

using EventStore.ClientAPI;
using Memstate.Configuration;

namespace Memstate.EventStore
{
    public class EventStoreProvider : IStorageProvider, IAsyncDisposable
    {
        private readonly IEventStoreConnection _connection;

        private readonly EventStoreSettings _eventStoreSettings;

        public EventStoreProvider()
        {
            var config = Config.Current;
            _eventStoreSettings = config.GetSettings<EventStoreSettings>();
            _connection = EventStoreConnection.Create(_eventStoreSettings.ConnectionString);
        }

        public Task Provision()
        {
            return _connection.ConnectAsync();
        }

        public IJournalReader CreateJournalReader()
        {
            return new EventStoreReader(_connection);
        }

        public IJournalWriter CreateJournalWriter()
        {
            return new EventStoreWriter(_connection);
        }


        public Task DisposeAsync()
        {
            _connection.Close();
            return Task.CompletedTask;
        }
    }
}