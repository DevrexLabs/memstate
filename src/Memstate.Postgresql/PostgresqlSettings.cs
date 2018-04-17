using System;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlSettings : Settings
    {
        public const string ConfigurationKey = "StorageProviders:Postgres";

        public const string DefaultConnectionString = "Host=localhost;Database=postgres;User ID=postgres;Password=postgres;";

        public const string InitSqlResourceName = "Memstate.Postgres.init_sql";

        private readonly MemstateSettings _memstateSettings;

        private string _connectionStringTemplate = DefaultConnectionString;

        public PostgresqlSettings(MemstateSettings settings)
            : base(settings, ConfigurationKey)
        {
            Ensure.NotNull(settings, nameof(settings));
            _memstateSettings = settings;
        }

        /// <summary>
        /// Password to connect to the database, overrides value in ConnectionString if set
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Username to connect to the database, overrides value in ConnectionString if set
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Name of the database, overrides value in ConnectionString if set
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Name of host to connect to, overrides value in ConnectionString if set
        /// </summary>
        public string Host { get; set; }

        public string ConnectionString
        {
            get
            {
                var builder = new NpgsqlConnectionStringBuilder(_connectionStringTemplate);
                if (Host != null) builder.Host = Host;
                if (Username != null) builder.Username = Username;
                if (Database != null) builder.Database = Database;
                if (Password != null) builder.Password = Password;
                return builder.ToString();
            }
            set
            {
                _connectionStringTemplate = value;
            }
        }

        public string TableSuffix { get; set; } = "_commands";

        public string SubscriptionStreamSuffix { get; set; } = "_notifications";

        public string Table => _memstateSettings?.StreamName + TableSuffix;

        public string SubscriptionStream => _memstateSettings?.StreamName + SubscriptionStreamSuffix;

        public int ReadBatchSize { get; set; } = 1024;

        public Lazy<string> InitSql => new Lazy<string>(() => GetEmbeddedResource(InitSqlResourceName));

        public override void Validate()
        {
            Ensure.NotNullOrEmpty(ConnectionString, nameof(ConnectionString));
            Ensure.NotNullOrEmpty(Table, nameof(Table));
            Ensure.NotNullOrEmpty(SubscriptionStream, nameof(SubscriptionStream));
        }
    }
}