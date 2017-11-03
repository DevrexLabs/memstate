namespace Memstate.Postgresql
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class PostgresqlSettings : MemstateSettings
    {
        public const string ConfigurationKey = "StorageProviders:Postgresql";

        public PostgresqlSettings(IConfiguration configuration)
            : base(configuration.GetSection(ConfigurationKey))
        {
        }

        public PostgresqlSettings(MemstateSettings settings)
            : this(settings.Configuration)
        {
        }

        public string ConnectionString { get; set; } =
            "Host=localhost;Database=postgres;User ID=postgres;Password=postgres;";


        public string Table { get; set; } = "memstate_commands";

        public string SubscriptionStream { get; set; } = "memstate_notifications";


        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentException("Property must have a value.", nameof(ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(Table))
            {
                throw new ArgumentException("Property must have a value.", nameof(Table));
            }

            if (string.IsNullOrWhiteSpace(SubscriptionStream))
            {
                throw new ArgumentException("Property must have a value.", nameof(SubscriptionStream));
            }
        }
    }
}
