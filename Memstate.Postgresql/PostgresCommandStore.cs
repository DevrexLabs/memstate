using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresCommandStore : IJournalWriter, IJournalReader, IJournalSubscriptionSource
    {
        private readonly Config _config;

        public PostgresCommandStore(Config config)
        {
            _config = config;
        }

        public void Send(Command command)
        {
            throw new NotImplementedException();
        }

        void IJournalWriter.Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            using (var connection = OpenConnection())
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

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            throw new NotImplementedException();
        }

        private JournalRecord ReadRecord(IDataRecord reader)
        {
            var recordNumber = (long) reader[0];
            var written = (DateTime) reader[1];
            var commandData = (byte[]) reader[2];

            var serializer = _config.GetSerializer();

            var command = (Command)serializer.Deserialize(commandData);
            
            return new JournalRecord(recordNumber, written, command);
        }

        private NpgsqlConnection OpenConnection()
        {
            // TODO: Fetch the connection string from the _config object once the structure
            // of the config is settled.
            var connectionString = "Server=localhost; Username=hagbard; Database=postgres";

            return new NpgsqlConnection(connectionString);
        }
    }
}