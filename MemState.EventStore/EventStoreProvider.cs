namespace Memstate.EventStore
{
    using global::EventStore.ClientAPI;

    public class EventStoreProvider : Provider
    {
        private const string DefaultConnectionString = "ConnectTo=tcp://admin:changeit@localhost:1113";

        private readonly IEventStoreConnection _connection;

        public EventStoreProvider(Settings config, IEventStoreConnection connection = null)
            : base(config)
        {
            if (connection == null)
            {
                connection = EventStoreConnection.Create(DefaultConnectionString);
                connection.ConnectAsync().Wait();
            }
            _connection = connection;
        }

        public override IJournalReader CreateJournalReader()
        {
            return new EventStoreReader(Config, _connection, Config.GetSerializer(), Config.StreamName);

        }

        public override IJournalWriter CreateJournalWriter()
        {
            return new EventStoreWriter(Config, _connection, Config.GetSerializer(), Config.StreamName);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new EventStoreSubscriptionSource(Config, _connection, Config.GetSerializer(), Config.StreamName);
        }

        public override void Dispose()
        {
            _connection.Close();
        }
    }
}