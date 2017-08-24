using System;
using System.Collections.Generic;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlWriter : BatchingJournalWriter
    {
        private const string ConfigurationSection = "Journal:Providers:Npgsql";

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

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly ISerializer _serializer;

        public PostgresqlWriter(Config config)
            : base(config)
        {
            _connectionString = config[$"{ConfigurationSection}:ConnectionString"];

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new ArgumentException("Property ConnectionString must have a value.", nameof(config));
            }

            _tableName = config[$"{ConfigurationSection}:TableName"];

            if (string.IsNullOrWhiteSpace(_tableName))
            {
                throw new ArgumentException("Property TableName must have a value.", nameof(config));
            }

            _serializer = config.GetSerializer();
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var sql = string.Format(InsertSql, _tableName);

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