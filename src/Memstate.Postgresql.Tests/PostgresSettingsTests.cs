using System;
using Memstate.Configuration;
using Npgsql;
using NUnit.Framework;

namespace Memstate.Postgres.Tests
{
    [TestFixture]
    public class PostgresSettingsTests
    {
        private PostgresSettings _settings;
        private EngineSettings _memstateSettings;

        [SetUp]
        public void Setup()
        {
            _memstateSettings = new EngineSettings();
            _settings = new PostgresSettings();
        }

        [Test]
        public void CanExtractInitSqlResource()
        {
            var initSql = _settings.InitSql.Value;
            Assert.True(!String.IsNullOrEmpty(initSql));
        }

        [Test]
        public void DefaultConnectionStringIsUsed()
        {
            var key = "Memstate:Postgres:Password";
            var defaultBuilder = new NpgsqlConnectionStringBuilder(PostgresSettings.DefaultConnectionString);

            //Appveyor workaround
            //test failed on Appveyor because the pgsql password env variable is set!
            defaultBuilder.Password = Environment.GetEnvironmentVariable(key) ?? defaultBuilder.Password;

            var actualBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.True(defaultBuilder.EquivalentTo(actualBuilder));
        }

        [Test]
        public void PasswordCanBeSetUsingEnvironmentVariable()
        {
            string key = "MEMSTATE_POSTGRES_PASSWORD";
            string value = "Password12!";
            Environment.SetEnvironmentVariable(key, value);
            var config = Config.Reset();
            var settings = config.GetSettings<PostgresSettings>();
            Assert.AreEqual(value, settings.Password);
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
        }

        [Test]
        public void PasswordOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Password = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Password);
        }

        [Test]
        public void UsernameOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Username = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Username);
        }

        [Test]
        public void DatabaseOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Database = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.AreEqual(expected, connectionStringBuilder.Database);
        }
    }
}