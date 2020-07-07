using Memstate.Configuration;

namespace Memstate.EventStore
{
    public class EventStoreSettings : Settings
    {
        public EventStoreSettings():base("Memstate.EventStore")
        {
            var config = Config.Current;
            var memstateSettings = config.GetSettings<EngineSettings>();
            StreamName = memstateSettings.StreamName;

        }

        public const string DefaultSerializer = "newtonsoft.json";

        public string ConnectionString { get; set; } = "ConnectTo=tcp://admin:changeit@localhost:1113";

        public string StreamName { get; set; }

        public string SerializerName => DefaultSerializer;

        public int EventsPerSlice { get; set; } = 1024;

        public void Validate()
        {
            Ensure.NotNullOrEmpty(ConnectionString, nameof(ConnectionString));
        }
    }
}