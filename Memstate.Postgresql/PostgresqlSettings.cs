namespace Memstate.Postgresql
{
    public class PostgresqlSettings : Settings
    {
        public const string ConfigurationKey = "StorageProviders:Postgresql";

        public PostgresqlSettings(MemstateSettings settings)
            : base(settings, ConfigurationKey)
        {
        }

        public string ConnectionString { get; set; } = "Host=localhost; Database=postgres; User ID=postgres; Password=postgres;";

        public string Table { get; set; } = "memstate_commands";

        public string SubscriptionStream { get; set; } = "memstate_notifications";

        public override void Validate()
        {
            Ensure.NotNullOrEmpty(ConnectionString, nameof(ConnectionString));
            Ensure.NotNullOrEmpty(Table, nameof(Table));
            Ensure.NotNullOrEmpty(SubscriptionStream, nameof(SubscriptionStream));
        }
    }
}