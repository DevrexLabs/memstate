using System;
using Dapper;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlClient
    {
        private readonly PostgresqlSettings _settings;

        public PostgresqlClient(PostgresqlSettings settings)
        {
            _settings = settings;
        }

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
            {
                connection.Execute(string.Format(sql, _settings.Table, _settings.SubscriptionStream));
            }
        }

        public PostgresqlWriter CreateWriter(ISerializer serializer)
        {
            var settings = _settings.Upgrade<PostgresqlWriterSettings>();

            settings.Serializer = serializer;

            //return new PostgresqlWriter(settings);
            throw new NotImplementedException();
        }

        public PostgresqlSubscriptionSource CreateSubscriptionSource()
        {
            var settings = _settings.Upgrade<PostgresqlSubscriptionSourceSettings>();
            
            return new PostgresqlSubscriptionSource(settings);
        }
    }
}