namespace Memstate.EventStore
{
    using global::EventStore.ClientAPI;

    public class EventStoreProvider : StorageProvider
    {
        private readonly IEventStoreConnection _connection;

        private readonly MemstateSettings _memstateSettings;
        private readonly EventStoreSettings _eventStoreSettings;


        public EventStoreProvider(MemstateSettings memstateSettings)
        {
            _memstateSettings = memstateSettings;
            _eventStoreSettings = new EventStoreSettings(memstateSettings);
            _connection = EventStoreConnection.Create(_eventStoreSettings.ConnectionString);
            _connection.ConnectAsync().Wait();
        }

        public override IJournalReader CreateJournalReader()
        {
            return new EventStoreReader(_memstateSettings, _eventStoreSettings, _connection);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            return new EventStoreWriter(_memstateSettings, _connection);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new EventStoreSubscriptionSource(_memstateSettings, _connection);
        }

        public override void Dispose()
        {
            _connection.Close();
        }
    }
}