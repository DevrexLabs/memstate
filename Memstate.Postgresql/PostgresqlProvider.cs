using Npgsql;

namespace Memstate.Postgresql
{
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
    ""id"" BIGINT NOT NULL PRIMARY KEY,
    ""written"" TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    ""command"" VARCHAR NOT NULL
);

CREATE SEQUENCE IF NOT EXISTS ""{1}_id_seq"" MINVALUE 0 OWNED BY ""{1}"".""id"";

ALTER TABLE ""{1}"" ALTER ""id"" SET DEFAULT nextval('{1}_id_seq');

DROP TRIGGER IF EXISTS ""{1}_notify_command"" ON ""{1}"";

CREATE TRIGGER ""{1}_notify_command""
    AFTER INSERT ON ""{1}""
    FOR EACH ROW EXECUTE PROCEDURE {1}_notify_command();
            ";

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