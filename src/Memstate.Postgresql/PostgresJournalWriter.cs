using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using Npgsql;
using Memstate.Logging;

namespace Memstate.Postgres
{
    public class PostgresJournalWriter : BatchingJournalWriter
    {
        private const string InsertSql = @"INSERT INTO {0} (""command"") VALUES {1};";

        private readonly ILog _logger;
        private readonly ISerializer _serializer;
        private readonly PostgresSettings _settings;
        private readonly string _tableName;

        public PostgresJournalWriter(Config config, string tableName)
            : base(config.GetSettings<EngineSettings>())
        {
            _settings = config.GetSettings<PostgresSettings>();
            _serializer = config.CreateSerializer();
            _logger = LogProvider.GetLogger(nameof(PostgresJournalWriter));
            _tableName = tableName;
        }

        protected override async Task OnCommandBatch(IEnumerable<Command> commands)
        {
            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            {
                await connection.OpenAsync();

                commands = commands.ToList();

                var count = commands.Count();

                _logger.Debug($"OnCommandBatch received {count} commands");

                var values = string.Join(",", Enumerable.Range(0, count).Select(i => $"(@{i})"));

                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = string.Format(InsertSql, _tableName, values);

                    commands.Select((c, i) => new {Index = i, Value = Convert.ToBase64String(_serializer.Serialize(c))})
                        .ToList()
                        .ForEach(item => sqlCommand.Parameters.AddWithValue($"@{item.Index}", item.Value));

                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}