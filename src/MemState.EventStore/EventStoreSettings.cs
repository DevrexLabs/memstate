namespace Memstate.EventStore
{
    public class EventStoreSettings : Settings
    {
        public override string Key { get; } = "Memstate:StorageProviders:EventStore";

        public const string DefaultSerializer = "newtonsoft.json";

        private readonly MemstateSettings _memstateSettings;

        public EventStoreSettings()
        {
            _memstateSettings = MemstateSettings.Current;
        }

        public string ConnectionString { get; set; } = "ConnectTo=tcp://admin:changeit@localhost:1113";

        public string StreamName => _memstateSettings?.StreamName;

        public string Serializer => DefaultSerializer;

        public int EventsPerSlice { get; set; } = 1024;

        public ISerializer CreateSerializer() => _memstateSettings.CreateSerializer(Serializer);

        public override void Validate()
        {
            Ensure.NotNullOrEmpty(ConnectionString, nameof(ConnectionString));
        }
    }
}