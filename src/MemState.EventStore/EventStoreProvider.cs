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
            //todo: consider opening the connection from Initialize
            _connection = EventStoreConnection.Create(_eventStoreSettings.ConnectionString);
            _connection.ConnectAsync().Wait();
        }

        public override IJournalReader CreateJournalReader()
        {
            return new EventStoreReader(_connection);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            return new EventStoreWriter(_connection);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new EventStoreSubscriptionSource(_connection);
        }

        public Task DisposeAsync()
        {
            return Task.Run((Action)_connection.Close);
        }
    }
}