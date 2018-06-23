using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Memstate.Logging;

namespace Memstate.Postgresql
{

    public class PostgresJournalWriter : BatchingJournalWriter
    {
        private const string InsertSql = @"INSERT INTO {0} (""command"") VALUES {1};";

        private readonly ILog _logger;

        private readonly ISerializer _serializer;

        private readonly PostgresSettings _settings;

        public PostgresJournalWriter(ISerializer serializer, PostgresSettings settings)
        {
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNull(settings, nameof(settings));
            _settings = settings;
            _serializer = serializer;
            _logger = LogProvider.GetCurrentClassLogger();
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            {
                connection.Open();

                commands = commands.ToList();

                var count = commands.Count();

                _logger.Debug($"OnCommandBatch received {count} commands");

                var values = string.Join(",", Enumerable.Range(0, count).Select(i => $"(@{i})"));

                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = string.Format(InsertSql, _settings.Table, values);

                    commands.Select((c, i) => new {Index = i, Value = Convert.ToBase64String(_serializer.Serialize(c))})
                        .ToList()
                        .ForEach(item => sqlCommand.Parameters.AddWithValue($"@{item.Index}", item.Value));

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}