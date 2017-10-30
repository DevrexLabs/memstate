using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresJournalReader : IJournalReader
    {
        private readonly ISerializer _serializer;
        private readonly PostgresqlSettings _settings;

        public PostgresJournalReader(Settings config, PostgresqlSettings settings)
        {
            _serializer = config.CreateSerializer();
            _settings = settings;
        }


        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    id,
    written,
    command
FROM
    commands
WHERE
    id >= @id
ORDER BY
    id ASC";

                command.Parameters.AddWithValue("@id", fromRecord);
                
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var journalRecord = ReadRecord(reader);
                        yield return journalRecord;
                    }
                }
            }
        }

        private JournalRecord ReadRecord(IDataRecord reader)
        {
            var recordNumber = (long)reader[0];
            var written = (DateTime)reader[1];
            var commandData = (byte[])reader[2];
            var command = (Command)_serializer.Deserialize(commandData);
            return new JournalRecord(recordNumber, written, command);
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_settings.ConnectionString);
        }

        public void Dispose()
        {
        }
    }
}