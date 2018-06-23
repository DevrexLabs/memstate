using Memstate.Logging;
using System.Threading.Tasks;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresProvider : StorageProvider
    {
        private readonly ILog _log;
        private bool _initialized;

        public PostgresProvider()
        {
            _log = LogProvider.GetCurrentClassLogger();
            Settings = Memstate.Settings.Read<PostgresSettings>();
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
            return new PostgresJournalReader(Settings);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            // todo: nextRecordNumber unused
            var serializer = MemstateSettings.Current.CreateSerializer();
            return new PostgresJournalWriter(serializer, Settings);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new PostgresSubscriptionSource(Settings);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}