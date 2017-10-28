
using Npgsql;

namespace Memstate.Postgresql
{

    public class PostgresqlProvider : StorageProvider
    {
        public const string ConfigurationSection = "StorageProviders:pgsql";

        private readonly PostgresqlSettings _settings;


        public PostgresqlProvider(Settings memstateSettings) : base(memstateSettings)
        {
            _settings = new PostgresqlSettings(memstateSettings.Configuration);
        }

        public PostgresqlSettings Settings => _settings;

        public void InitializeDatabase()
        {
            const string sql = @"
CREATE FUNCTION notify_command() RETURNS TRIGGER AS $$
    DECLARE
        data json;

    BEGIN
        data = row_to_json(NEW);

        PERFORM pg_notify('{0}', notification::text);

        RETURN NULL;
    END;
$$ LANGUAGE plpgsql;

CREATE TABLE ""{1}"" (
    ""id"" BIGSERIAL NOT NULL PRIMARY KEY,
    ""created_on"" TIMESTAMP WITH OUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    ""command_id"" UUID NOT NULL,
    ""type"" VARCHAR(4000) NOT NULL,
    ""data"" BYTEA NOT NULL
);

CREATE TRIGGER ""{1}_notify_command""
    AFTER INSERT ON ""{1}""
    FOR EACH ROW EXECUTE PROCEDURE notify_command();
            ";

            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            using(var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = string.Format(sql, _settings.Table, _settings.SubscriptionStream);
                command.ExecuteNonQuery();
            }
        }

        public override IJournalReader CreateJournalReader()
        {
            return new PostgresJournalReader(Config, _settings);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            // todo: nextRecordNumber unused

            return new PostgresqlWriter(Config, _settings);
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