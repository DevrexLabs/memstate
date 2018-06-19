using Memstate.Logging;
using System.Threading.Tasks;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlProvider : StorageProvider
    {
        private readonly ILog _log;
        
        private readonly MemstateSettings _settings;

        private bool _initialized;

        public PostgresqlProvider(MemstateSettings settings)
        {
            _log = LogProvider.GetCurrentClassLogger();
            _settings = settings;
            Settings = new PostgresqlSettings(settings);
        }

        public PostgresqlSettings Settings { get; }

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
            return new PostgresqlJournalReader(_settings);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            // todo: nextRecordNumber unused

            return new PostgresqlJournalWriter(_settings);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new PostgresqlSubscriptionSource(_settings);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}