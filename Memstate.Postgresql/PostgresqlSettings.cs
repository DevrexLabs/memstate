namespace Memstate.Postgresql
{
    public class PostgresqlSettings : Settings
    {
        public const string ConfigurationKey = "StorageProviders:Postgresql";
        public const string DefaultConnectionString = "Host=localhost; Database=postgres; User ID=postgres; Password=postgres;";

        private readonly MemstateSettings _memstateSettings;

        public PostgresqlSettings(MemstateSettings settings)
            : base(settings, ConfigurationKey)
        {
            Ensure.NotNull(settings, nameof(settings));
            _memstateSettings = settings;
        }

        public string ConnectionString { get; set; } = DefaultConnectionString;

        public string TableSuffix { get; set; } = "_commands";

        public string SubscriptionStreamSuffix { get; set; } = "_notifications";

        public string Table => _memstateSettings?.StreamName + TableSuffix;

        public string SubscriptionStream => _memstateSettings?.StreamName + SubscriptionStreamSuffix;

        public override void Validate()
        {
            Ensure.NotNullOrEmpty(ConnectionString, nameof(ConnectionString));
            Ensure.NotNullOrEmpty(Table, nameof(Table));
            Ensure.NotNullOrEmpty(SubscriptionStream, nameof(SubscriptionStream));
        }
    }
}