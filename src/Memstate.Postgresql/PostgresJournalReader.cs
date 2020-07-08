using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate.Postgres
{
    public class PostgresJournalReader : JournalReader
    {
        private const string SelectSql = 
            @"SELECT id, written, command FROM {0}
              WHERE id >= @id ORDER BY id ASC";

        private readonly ISerializer _serializer;
        
        private readonly PostgresSettings _settings;
        private readonly string _tableName;

        public PostgresJournalReader(Config config, string tableName)
        {
            _tableName = tableName;
            _settings = config.GetSettings<PostgresSettings>();
            _serializer = config.CreateSerializer();
        }

        public override IEnumerable<JournalRecord> ReadRecords(long fromRecord)
        {
            using (var connection = OpenConnection())
            {
                do
                {
                    using (var command = connection.CreateCommand())
                    {
                        var recordsRead = 0;
                        command.CommandText = string.Format(SelectSql, _tableName);
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

                        if (recordsRead == 0) break;
                    }
                }
                while (true);
            }
        }

        public override Task DisposeAsync() => Task.CompletedTask;

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