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

        public PostgresProvider()
        {
            _log = LogProvider.GetCurrentClassLogger();
            Settings = Config.Current.GetSettings<PostgresSettings>();
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

            var sql = Settings.InitSql.Value;
            
            using (var connection = new NpgsqlConnection(Settings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                await connection.OpenAsync();
                command.CommandText = string.Format(sql, Settings.SubscriptionStream, Settings.Table);
                await command.ExecuteNonQueryAsync();
            }
            _initialized = true;
        }

        public IJournalReader CreateJournalReader()
        {
            return new PostgresJournalReader(Settings);
        }

        public IJournalWriter CreateJournalWriter()
        {
            var serializer = Config.Current.CreateSerializer();
            return new PostgresJournalWriter(serializer, Settings);
        }
        
        public Task DisposeAsync() => Task.CompletedTask;
    }
}