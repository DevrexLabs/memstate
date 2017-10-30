namespace Memstate.Postgresql.Tests
{
    using System;
    using System.Collections.Generic;
    using Memstate.Postgresql.Tests.Domain;
    using Npgsql;
    using Xunit;

    public class JournalReaderTests : IDisposable
    {
        private const string ConnectionString = "Host=localhost; Username=hagbard; Database=postgres";

        private readonly PostgresJournalReader _journalReader;
        private readonly ISerializer _serializer;
        private readonly IJournalWriter _journalWriter;

        public JournalReaderTests()
        {
            var config = new Settings();
            var pgsqlSettings = new PostgresqlSettings(config) { ConnectionString = ConnectionString };

            _journalReader = new PostgresJournalReader(config, pgsqlSettings);
            _journalWriter = new PostgresqlWriter(config, pgsqlSettings);
            _serializer = config.CreateSerializer();
            ClearDatabase();
        }

        [Fact]
        public void CanRead()
        {
            var create = new Create(Guid.NewGuid(), "Create a Postgresql driver for Memstate");
            InsertCommand(_serializer.Serialize(create));
            var journalRecords = _journalReader.GetRecords();
            Assert.Single(journalRecords);
        }

        [Fact]
        public void CanWrite()
        {
            var create = new Create(Guid.NewGuid(), "Create a Postgresql driver for Memstate");
            _journalWriter.Send(create);
            var journalRecords = GetJournalRecords();
            Assert.Single(journalRecords);
        }

        public void Dispose()
        {
            _journalReader.Dispose();
            _journalWriter.Dispose();
        }

        private static void ClearDatabase()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "TRUNCATE TABLE commands;";
                command.ExecuteNonQuery();
            }
        }

        private static void InsertCommand(byte[] data)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
INSERT INTO
    commands
(
    command
)
VALUES
(
    @command
);";

                command.Parameters.AddWithValue("@command", data);
                Assert.Equal(1, command.ExecuteNonQuery());
            }
        }

        private static List<JournalRecord> GetJournalRecords()
        {
            var journalRecords = new List<JournalRecord>();
            
            using (var connection = new NpgsqlConnection(ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
SELECT
    id,
    written
FROM
    commands
ORDER BY
    id ASC;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var journalRecord = new JournalRecord((long)reader[0], (DateTime)reader[1], null);
                        
                        journalRecords.Add(journalRecord);
                    }
                }
            }

            return journalRecords;
        }
    }
}