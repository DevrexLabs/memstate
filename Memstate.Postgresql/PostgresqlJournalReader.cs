using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlJournalReader : IJournalReader
    {
        private const string SelectSql = @"
SELECT
    id,
    written,
    command
FROM
    {0}
WHERE
    id >= @id
ORDER BY
    id ASC";

        private readonly ISerializer _serializer;
        private readonly PostgresqlSettings _settings;

        public PostgresqlJournalReader(MemstateSettings memstateSettings)
        {
            Ensure.NotNull(memstateSettings, nameof(memstateSettings));
            _settings = new PostgresqlSettings(memstateSettings);
            _serializer = memstateSettings.CreateSerializer();
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(SelectSql, _settings.Table);

                command.Parameters.AddWithValue("@id", fromRecord);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadRecord(reader);
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        private JournalRecord ReadRecord(IDataRecord reader)
        {
            var recordNumber = (long) reader[0];
            var written = (DateTime) reader[1];
            var commandData = Convert.FromBase64String((string) reader[2]);
            var command = (Command) _serializer.Deserialize(commandData);

            return new JournalRecord(recordNumber, written, command);
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_settings.ConnectionString);
        }
    }
}