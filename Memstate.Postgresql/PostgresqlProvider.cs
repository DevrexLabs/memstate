using Microsoft.Extensions.Logging;

namespace Memstate.Postgresql
{
    using System.Threading.Tasks;
    using Npgsql;

    public class PostgresqlProvider : StorageProvider
    {
        private readonly ILogger<PostgresqlProvider> _log;
        private readonly MemstateSettings _settings;
        private readonly PostgresqlSettings _postgreSqlSettings;
        private bool _initialized;

        public PostgresqlProvider(MemstateSettings settings)
        {
            _log = settings.CreateLogger<PostgresqlProvider>();
            _settings = settings;
            _postgreSqlSettings = new PostgresqlSettings(settings);
        }

        public PostgresqlSettings Settings => _postgreSqlSettings;

        public override void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            
            var sql = _postgreSqlSettings.InitSql.Value;
            using (var connection = new NpgsqlConnection(_postgreSqlSettings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = string.Format(sql, _postgreSqlSettings.SubscriptionStream, _postgreSqlSettings.Table);
                _log.LogTrace($"Executing SQL '{command.CommandText}'");
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