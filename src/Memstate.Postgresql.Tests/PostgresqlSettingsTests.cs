using System;
using Npgsql;
using NUnit.Framework;

namespace Memstate.Postgresql.Tests
{
    [TestFixture]
    public class PostgresqlSettingsTests
    {
        private  PostgresSettings _settings;
        private  MemstateSettings _memstateSettings;

        [SetUp]
        public void Setup()
        {
            _memstateSettings = new MemstateSettings();
            _settings = new PostgresSettings(_memstateSettings);
        }

        [Test]
        public void CanExtractInitSqlResource()
        {
            foreach (var resourceName in _settings.GetEmbeddedResourceNames())
            {
                Console.WriteLine(resourceName);
            }

            var initSql = _settings.InitSql.Value;

            Assert.AreEqual("CREATE", initSql.Substring(0,6));
        }

        [Test]
        public void DefaultConnectionStringIsUsed()
        {
            var key = "Memstate:StorageProviders:Postgres:Password";
            var defaultBuilder = new NpgsqlConnectionStringBuilder(PostgresSettings.DefaultConnectionString);

            //Appveyor workaround
            //test failed on Appveyor because the pgsql password env variable is set
            defaultBuilder.Password = Environment.GetEnvironmentVariable(key) ?? defaultBuilder.Password;

            var actualBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.True(defaultBuilder.EquivalentTo(actualBuilder));
        }

        [Test]
        public void TableNameEndsWithSuffix()
        {
            Assert.True(_settings.Table.EndsWith(_settings.TableSuffix));
        }

        [Test]
        public void TableNameStartsWithStreamName()
        {
            Assert.True(_settings.Table.StartsWith(_memstateSettings.StreamName));
        }

        [Test]
        public void HostOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Host = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Host);
            Assert.True(connectionStringBuilder.ToString().Contains(expected));
        }

        [Test]
        public void PasswordOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Password = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Password);
            Assert.True(connectionStringBuilder.ToString().Contains(expected));
        }

        [Test]
        public void UsernameOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Username = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Username);
            Assert.True(connectionStringBuilder.ToString().Contains(expected));
        }

        [Test]
        public void DatabaseOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Database = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Database);
            Assert.True(connectionStringBuilder.ToString().Contains(expected));
        }

        [Test]
        public void PasswordFromArgumentsOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            var settings = new MemstateSettings("--Memstate:StorageProviders:Postgres:Password", expected);
            var pgSettings = new PostgresSettings(settings);
            Assert.AreEqual(expected, pgSettings.Password);
        }

        [Test, Ignore("Interferes with same ENV var on appveyor!")]
        public void PasswordFromEnvironmentVariableOverridesConnectionString()
        {
            string key = "Memstate:StorageProviders:Postgres:Password";
            var expected = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, expected);
            var settings = new MemstateSettings();
            var pgSettings = new PostgresSettings(settings);
            Assert.AreEqual(expected, pgSettings.Password);
            Environment.SetEnvironmentVariable(key, null);
        }
    }
}