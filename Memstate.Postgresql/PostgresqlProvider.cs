using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlProvider : StorageProvider
    {
        private readonly PostgresqlSettings _settings;

        public PostgresqlProvider(PostgresqlSettings settings)
            : base(settings)
        {
            _settings = settings;
        }

        public PostgresqlProvider(Settings memstateSettings)
            : this(new PostgresqlSettings(memstateSettings))
        {
        }

        public PostgresqlSettings Settings => _settings;

        public override void Initialize()
        {
            const string sql = @"
CREATE OR REPLACE FUNCTION {1}_notify_command() RETURNS TRIGGER AS $$
    DECLARE
        data json;

    BEGIN
        data = row_to_json(NEW);

        PERFORM pg_notify('{0}', data::text);

        RETURN NULL;
    END;
$$ LANGUAGE plpgsql;

CREATE TABLE IF NOT EXISTS ""{1}"" (
    ""id"" BIGSERIAL NOT NULL PRIMARY KEY,
    ""written"" TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    ""command"" BYTEA NOT NULL
);

DROP TRIGGER IF EXISTS ""{1}_notify_command"" ON ""{1}"";

CREATE TRIGGER ""{1}_notify_command""
    AFTER INSERT ON ""{1}""
    FOR EACH ROW EXECUTE PROCEDURE {1}_notify_command();
            ";

            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = string.Format(sql, _settings.SubscriptionStream, _settings.Table);
                command.ExecuteNonQuery();
            }
        }

        public override IJournalReader CreateJournalReader()
        {
            return new PostgresqlJournalReader(Config, _settings);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            // todo: nextRecordNumber unused

            return new PostgresqlJournalWriter(Config, _settings);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new PostgresqlSubscriptionSource(Config, _settings);
        }

        public override void Dispose()
        {
        }
    }
}