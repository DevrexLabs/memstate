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
        private string _tableName;

        [SetUp]
        public async System.Threading.Tasks.Task Setup()
        {
            var config = Config.CreateDefault();

            _provider = new PostgresProvider(config);
            await _provider.Provision();

            _journalReader = _provider.CreateJournalReader();
            _journalWriter = _provider.CreateJournalWriter();

            _serializer = config.CreateSerializer();
            _tableName = config.GetSettings<EngineSettings>().StreamName + "_journal";
        }

        [Test]
        public void CanRead()
        {
            var create = new Create(Guid.NewGuid(), "Resolve a Postgresql driver for Memstate");

            InsertCommand(_serializer.Serialize(create));

            var journalRecords = _journalReader.ReadRecords();

            Assert.AreEqual(1, journalRecords.Count());
        }

        [Test]
        public void CanWrite()
        {
            var create = new Create(Guid.NewGuid(), "Resolve a Postgresql driver for Memstate");

            _journalWriter.Write(create);

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

                command.CommandText = $"INSERT INTO {_tableName} (command) VALUES(@command);";

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

                command.CommandText = $"SELECT id, written FROM {_tableName} ORDER BY id ASC;";

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