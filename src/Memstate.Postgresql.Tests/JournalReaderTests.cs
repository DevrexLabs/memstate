using System.Threading;
using System;
using System.Collections.Generic;
using Memstate.Postgresql.Tests.Domain;
using Npgsql;
using Xunit;

namespace Memstate.Postgresql.Tests
{
    public class JournalReaderTests
    {
        private readonly PostgresProvider _provider;

        private readonly IJournalReader _journalReader;

        private readonly IJournalWriter _journalWriter;

        private readonly ISerializer _serializer;

        public JournalReaderTests()
        {
            var settings = new MemstateSettings().WithRandomSuffixAppendedToStreamName();
            _provider = new PostgresProvider(settings);
            _provider.Initialize();

            _journalReader = _provider.CreateJournalReader();
            _journalWriter = _provider.CreateJournalWriter(0);

            _serializer = settings.CreateSerializer();
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

            Thread.Sleep(500);

            var journalRecords = GetJournalRecords();

            Assert.Single(journalRecords);
        }

        private void InsertCommand(byte[] data)
        {
            using (var connection = new NpgsqlConnection(_provider.Settings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = string.Format("INSERT INTO {0} (command) VALUES(@command);",
                    _provider.Settings.Table);

                command.Parameters.AddWithValue("@command", Convert.ToBase64String(data));

                Assert.Equal(1, command.ExecuteNonQuery());
            }
        }

        private IEnumerable<JournalRecord> GetJournalRecords()
        {
            var journalRecords = new List<JournalRecord>();

            using (var connection = new NpgsqlConnection(_provider.Settings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = string.Format(
                    "SELECT id, written FROM {0} ORDER BY id ASC;",
                    _provider.Settings.Table);

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