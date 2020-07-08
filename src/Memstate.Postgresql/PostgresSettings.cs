using System;
using System.IO;
using System.Reflection;
using Npgsql;

namespace Memstate.Postgres
{
    public class PostgresSettings : Settings
    {
        public const string DefaultConnectionString = "Host=localhost;Database=postgres;User ID=postgres;Password=postgres;";

        private const string InitSqlResourceName = "Memstate.Postgres.init_sql";

        private string _connectionStringTemplate = DefaultConnectionString;

        public PostgresSettings() : base("Memstate.Postgres") { }

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
            set => _connectionStringTemplate = value;
        }

        /// <summary>
        /// String.Format(EngineSettings.StreamName) will be applied to
        /// obtain the table name
        /// </summary>
        public string TableNameTemplate { get; set; } = "{0}_journal";


        public string RenderTableNameTemplate(string streamName)
            => String.Format(TableNameTemplate, streamName);

        /// <summary>
        /// Number of records to read per SELECT statement
        /// </summary>
        /// <value>The size of the read batch.</value>
        public int ReadBatchSize { get; set; } = 1024;

        public Lazy<string> InitSql
            => new Lazy<string>(() => GetEmbeddedResource(InitSqlResourceName));
        
        private string GetEmbeddedResource(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();

            using (var stream = asm.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}