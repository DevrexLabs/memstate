namespace Memstate.EventStore
{
    using global::EventStore.ClientAPI;

    public class EventStoreProvider : StorageProvider
    {
        private const string DefaultConnectionString = "ConnectTo=tcp://admin:changeit@localhost:1113";

        private readonly IEventStoreConnection _connection;

        public EventStoreProvider(Settings settings)
            : this(new EventStoreSettings(settings))
        {
        }

        public EventStoreProvider(Settings settings, IEventStoreConnection connection)
            : base(settings)
        {
            if (connection == null)
            {
                connection = EventStoreConnection.Create(DefaultConnectionString);
                connection.ConnectAsync().Wait();
            }

            _connection = connection;
        }

        public EventStoreProvider(EventStoreSettings settings)
            : base(settings)
        {
            _connection = EventStoreConnection.Create(settings.ConnectionString);
            _connection.ConnectAsync().Wait();
        }

        public override IJournalReader CreateJournalReader()
        {
            return new EventStoreReader(Config, _connection, Config.CreateSerializer(), Config.StreamName);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            return new EventStoreWriter(Config, _connection, Config.CreateSerializer(), Config.StreamName);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new EventStoreSubscriptionSource(Config, _connection, Config.CreateSerializer(), Config.StreamName);
        }

        public override void Dispose()
        {
            _connection.Close();
        }
    }
}