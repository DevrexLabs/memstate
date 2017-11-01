using System;
using System.Collections.Generic;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlJournalWriter : BatchingJournalWriter
    {
        private const string InsertSql = @"
INSERT INTO {0}
(
    ""command""
)
VALUES
(
    @command
);";

        private readonly ISerializer _serializer;
        private readonly PostgresqlSettings _settings;

        public PostgresqlJournalWriter(PostgresqlSettings settings)
            : base(settings)
        {
            _settings = settings;
            _serializer = _settings.CreateSerializer();
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                
                var sql = string.Format(InsertSql, _settings.Table);

                foreach (var command in commands)
                {
                    using (var sqlCommand = connection.CreateCommand())
                    {
                        var commandData = _serializer.Serialize(command);

                        var commandDataString = Convert.ToBase64String(commandData);
                        
                        sqlCommand.CommandText = sql;
                        sqlCommand.Parameters.AddWithValue("@command", commandDataString);

                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}