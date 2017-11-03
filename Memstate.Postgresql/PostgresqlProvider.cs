using Npgsql;

namespace Memstate.Postgresql
{
    using System.IO;
    using System.Reflection;

    public class PostgresqlProvider : StorageProvider
    {
        private readonly MemstateSettings _settings;

        private readonly PostgresqlSettings _pgSettings;

        public PostgresqlProvider(MemstateSettings settings)
        {
            _settings = settings;
            _pgSettings = new PostgresqlSettings(settings);
        }

        public PostgresqlSettings Settings => _pgSettings;

        public override void Initialize()
        {
            var sql = _pgSettings.InitSql.Value;
            using (var connection = new NpgsqlConnection(_pgSettings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = string.Format(sql, _pgSettings.SubscriptionStream, _pgSettings.Table);
                command.ExecuteNonQuery();
            }
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

        public override void Dispose()
        {
        }
    }
}