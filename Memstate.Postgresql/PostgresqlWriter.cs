using System;
using System.Collections.Generic;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlWriter : BatchingJournalWriter
    {
        private const string InsertSql = @"
INSERT INTO {0}
(
    ""command_id"",
    ""type"",
    ""data""
)
VALUES
(
    @commandId,
    @type,
    @data
);";

        private readonly ISerializer _serializer;
        private readonly PostgresqlSettings _settings;

        public PostgresqlWriter(Settings config, PostgresqlSettings settings)
            : base(config)
        {
            _settings = settings;
            _serializer = config.CreateSerializer();
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            {
                var sql = string.Format(InsertSql, _settings.Table);

                foreach (var command in commands)
                {
                    using (var sqlCommand = connection.CreateCommand())
                    {
                        var data = _serializer.Serialize(command);
                        
                        sqlCommand.CommandText = sql;
                        sqlCommand.Parameters.AddWithValue("commandId", command.Id);
                        sqlCommand.Parameters.AddWithValue("type", command.GetType().ToString());
                        sqlCommand.Parameters.AddWithValue("data", data);
                    }
                }
            }
        }
    }
}