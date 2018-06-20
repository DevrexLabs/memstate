namespace Memstate
{
    public class StorageProviders : Providers<StorageProvider>
    {
        public const string EventStore 
            = "Memstate.EventStore.EventStoreProvider, Memstate.EventStore";

        public const string Postgres =
                    "Memstate.Postgres.PostgresProvider, Memstate.Postgres";

        public StorageProviders()
        {
            Register("file", s => new FileStorageProvider(s));
            Register("EventStore", s => InstanceFromTypeName(EventStore, s));
            Register("Postgres", s => InstanceFromTypeName(Postgres, s));
        }
    }
}