using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace Memstate.Postgresql
{
    using System.Threading.Tasks;

    public class PostgresJournalReader : IJournalReader
    {
        private const string SelectSql = @"SELECT id, written, command FROM {0}
                                           WHERE id >= @id
                                           ORDER BY id ASC";

        private readonly ISerializer _serializer;
        
        private readonly PostgresSettings _settings;

        public PostgresJournalReader(MemstateSettings memstateSettings)
            : this(new PostgresSettings(memstateSettings))
        {
        }

        public PostgresJournalReader(PostgresSettings settings)
        {
            Ensure.NotNull(settings, nameof(settings));

            _settings = settings;
            _serializer = settings.Memstate.CreateSerializer();
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            using (var connection = OpenConnection())
            {
                do
                {
                    using (var command = connection.CreateCommand())
                    {
                        var recordsRead = 0;

                        command.CommandText = string.Format(SelectSql, _settings.Table);

                        command.Parameters.AddWithValue("id", fromRecord);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recordsRead++;

                                var record = ReadRecord(reader);

                                fromRecord++;

                                yield return record;
                            }
                        }

                        if (recordsRead == 0)
                        {
                            break;
                        }
                    }
                }
                while (true);
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private JournalRecord ReadRecord(IDataRecord reader)
        {
            var recordNumber = (long) reader[0];
            var written = (DateTime) reader[1];
            var commandData = Convert.FromBase64String((string) reader[2]);
            var command = (Command) _serializer.Deserialize(commandData);

            return new JournalRecord(recordNumber, written, command);
        }

        private NpgsqlConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(_settings.ConnectionString);

            connection.Open();

            return connection;
        }
    }
}