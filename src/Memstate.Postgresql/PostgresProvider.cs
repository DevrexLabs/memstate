using Memstate.Logging;
using System.Threading.Tasks;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresProvider : StorageProvider
    {
        private readonly ILog _log;
        
        private readonly MemstateSettings _settings;

        private bool _initialized;

        public PostgresProvider(MemstateSettings settings)
        {
            _log = LogProvider.GetCurrentClassLogger();
            _settings = settings;
            Settings = new PostgresSettings(settings);
        }

        public PostgresSettings Settings { get; }

        public override void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            
            var sql = Settings.InitSql.Value;
            
            using (var connection = new NpgsqlConnection(Settings.ConnectionString))
                
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                
                command.CommandText = string.Format(sql, Settings.SubscriptionStream, Settings.Table);
                
                _log.Trace($"Executing SQL '{command.CommandText}'");
                
                command.ExecuteNonQuery();
            }

            _initialized = true;
        }

        public override IJournalReader CreateJournalReader()
        {
            return new PostgresJournalReader(_settings);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            // todo: nextRecordNumber unused

            return new PostgresJournalWriter(_settings);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new PostgresSubscriptionSource(_settings);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}