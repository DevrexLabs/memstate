using Memstate.Logging;
using System.Threading.Tasks;
using Npgsql;
using Memstate.Configuration;
using Npgsql.Logging;
using System.Diagnostics;

namespace Memstate.Postgres
{
    public class PostgresProvider : IStorageProvider
    {
        private readonly ILog _log;
        private bool _initialized;

        private readonly string _tableName;
        private readonly Config _config;

        public PostgresProvider(Config config)
        {
            _config = config;
            _log = LogProvider.GetLogger(nameof(PostgresProvider));
            Settings = config.GetSettings<PostgresSettings>();
            var engineSettings = config.GetSettings<EngineSettings>();
            _tableName = Settings.RenderTableNameTemplate(engineSettings.StreamName);
        }

        static PostgresProvider()
        {
            EnableNpgsqlDebugLogging();
        }

        [Conditional("PGTRACE")]
        private static void EnableNpgsqlDebugLogging()
        {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);
            NpgsqlLogManager.IsParameterLoggingEnabled = true;
        }

        public PostgresSettings Settings { get; }

        public async Task Provision()
        {
            if (_initialized) return;
            _log.Debug("Initializing...");

            using (var connection = new NpgsqlConnection(Settings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                await connection.OpenAsync().NotOnCapturedContext();
                command.CommandText = GetSql();
                await command.ExecuteNonQueryAsync().NotOnCapturedContext();
            }
            _initialized = true;
        }

        private string GetSql()
        {
            var sql = Settings.InitSql.Value;
            return string.Format(sql, _tableName);
        }

        public IJournalReader CreateJournalReader()
        {
            return new PostgresJournalReader(_config, _tableName);
        }

        public IJournalWriter CreateJournalWriter()
        {
            return new PostgresJournalWriter(_config, _tableName);
        }
        
        public Task DisposeAsync() => Task.CompletedTask;
    }
}