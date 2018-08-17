using System.Threading;
using System;
using System.Collections.Generic;
using Memstate.Postgres.Tests.Domain;
using Npgsql;
using NUnit.Framework;
using System.Linq;
using Memstate.Configuration;

namespace Memstate.Postgres.Tests
{
    [TestFixture]
    public class JournalReaderTests
    {
        private  PostgresProvider _provider;
        private  IJournalReader _journalReader;
        private  IJournalWriter _journalWriter;
        private  ISerializer _serializer;

        [SetUp]
        public void Setup()
        {
            var cfg = Config.Reset();
            cfg.Resolve<MemstateSettings>()
               .WithRandomSuffixAppendedToStreamName();

            _provider = new PostgresProvider();
            _provider.Initialize();

            _journalReader = _provider.CreateJournalReader();
            _journalWriter = _provider.CreateJournalWriter(0);

            _serializer = Config.Current.CreateSerializer();
        }

        [Test]
        public void CanRead()
        {
            var create = new Create(Guid.NewGuid(), "Resolve a Postgresql driver for Memstate");

            InsertCommand(_serializer.Serialize(create));

            var journalRecords = _journalReader.GetRecords();

            Assert.AreEqual(1, journalRecords.Count());
        }

        [Test]
        public void CanWrite()
        {
            var create = new Create(Guid.NewGuid(), "Resolve a Postgresql driver for Memstate");

            _journalWriter.Send(create);

            Thread.Sleep(500);

            var journalRecords = GetJournalRecords();

            Assert.AreEqual(1, journalRecords.Count());
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

                Assert.AreEqual(1, command.ExecuteNonQuery());
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