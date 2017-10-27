using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Memstate.Postgresql.Tests.Domain;
using Npgsql;

namespace Memstate.Postgresql.Tests
{
    public class JournalReaderTests
    {
        private const string ConnectionString = "Host=localhost; Username=hagbard; Database=postgres";

        private readonly Config _config;

        private PostgresCommandStore _commandStore;

        public JournalReaderTests()
        {
            var config = new Config
            {
                ["Providers:Postgresql:ConnectionString"] = ConnectionString
            };

            var commandStore = new PostgresCommandStore(config);

            _config = config;
            _commandStore = commandStore;
        }

        [Fact]
        public void CanRead()
        {
            ClearDatabase();
            
            var serializer = _config.GetSerializer();

            var create = new Create(Guid.NewGuid(), "Create a Postgresql driver for Memstate");

            InsertCommand(serializer.Serialize(create));

            var journalRecords = _commandStore.GetRecords();
            
            Assert.Equal(1, journalRecords.Count());
        }

        [Fact]
        public void CanWrite()
        {
            ClearDatabase();
            
            var create = new Create(Guid.NewGuid(), "Create a Postgresql driver for Memstate");
            
            _commandStore.Send(create);

            var journalRecords = GetJournalRecords();
            
            Assert.Equal(1, journalRecords.Count);
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
                        var journalRecord = new JournalRecord((long) reader[0], (DateTime) reader[1], null);
                        
                        journalRecords.Add(journalRecord);
                    }
                }
            }

            return journalRecords;
        }
    }
}