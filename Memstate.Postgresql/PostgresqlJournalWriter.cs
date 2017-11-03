using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlJournalWriter : BatchingJournalWriter
    {
        private const string InsertSql = @"INSERT INTO {0} (""command"") VALUES {1};";

        private readonly ISerializer _serializer;
        private readonly PostgresqlSettings _settings;

        public PostgresqlJournalWriter(MemstateSettings settings)
            : base(settings)
        {
            Ensure.NotNull(settings, nameof(settings));
            _serializer = settings.CreateSerializer();
            _settings = new PostgresqlSettings(settings);
        }

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            using (var connection = new NpgsqlConnection(_settings.ConnectionString))
            {
                connection.Open();

                commands = commands.ToList();

                var count = commands.Count();

                var values = string.Join(",", Enumerable.Range(0, count).Select(i => $"(@{i})"));

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(InsertSql, _settings.Table, values);
                    
                    commands.Select((c,i) => new { Index = i, Value = Convert.ToBase64String(_serializer.Serialize(command))})
                        .ToList()
                        .ForEach(item => command.Parameters.AddWithValue($"@{item.Index}", item.Value));

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}