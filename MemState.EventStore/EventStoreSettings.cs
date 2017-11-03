namespace Memstate.EventStore
{
    public class EventStoreSettings : Settings
    {
        public const string ConfigurationKey = "StorageProviders:EventStore";

        public EventStoreSettings(MemstateSettings settings)
            : base(settings, ConfigurationKey)
        {
        }

        public string ConnectionString { get; set; } = "ConnectTo=tcp://admin:changeit@localhost:1113";

        public string StreamName { get; set; } = "memstate";

        public int EventsPerSlice { get; set; } = 1024;

        public override void Validate()
        {
            Ensure.NotNullOrEmpty(ConnectionString, nameof(ConnectionString));
        }
    }
}