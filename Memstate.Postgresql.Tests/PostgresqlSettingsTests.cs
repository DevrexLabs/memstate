using System;
using Xunit;
using Xunit.Abstractions;
using Npgsql;

namespace Memstate.Postgresql.Tests
{
    public class PostgresqlSettingsTests
    {
        private readonly PostgresqlSettings _settings;
        private readonly MemstateSettings _memstateSettings;
        private readonly ITestOutputHelper _log;

        public PostgresqlSettingsTests(ITestOutputHelper log)
        {
            _log = log;
            _memstateSettings = new MemstateSettings();
            _settings = new PostgresqlSettings(_memstateSettings);
        }

        [Fact]
        public void CanExtractInitSqlResource()
        {
            foreach (var resourceName in _settings.GetEmbeddedResourceNames())
            {
                _log.WriteLine(resourceName);
            }

            var initSql = _settings.InitSql.Value;

            Assert.StartsWith("CREATE", initSql);
        }

        [Fact]
        public void DefaultConnectionStringIsUsed()
        {
            var defaultBuilder = new NpgsqlConnectionStringBuilder(PostgresqlSettings.DefaultConnectionString);
            var actualBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.True(defaultBuilder.EquivalentTo(actualBuilder));
        }

        [Fact]
        public void TableNameEndsWithSuffix()
        {
            Assert.EndsWith(_settings.TableSuffix, _settings.Table);
        }

        [Fact]
        public void TableNameStartsWithStreamName()
        {
            Assert.StartsWith(_memstateSettings.StreamName, _settings.Table);
        }

        [Fact]
        public void HostOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Host = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.Equal(expected, connectionStringBuilder.Host);
            Assert.Contains(expected, connectionStringBuilder.ToString());
        }

        [Fact]
        public void PasswordOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Password = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.Equal(expected, connectionStringBuilder.Password);
            Assert.Contains(expected, connectionStringBuilder.ToString());
        }

        [Fact]
        public void UsernameOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Username = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.Equal(expected, connectionStringBuilder.Username);
            Assert.Contains(expected, connectionStringBuilder.ToString());
        }

        [Fact]
        public void DatabaseOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            _settings.Database = expected;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_settings.ConnectionString);
            Assert.Equal(expected, connectionStringBuilder.Database);
            Assert.Contains(expected, connectionStringBuilder.ToString());
        }

        [Fact]
        public void PasswordFromArgumentsOverridesConnectionString()
        {
            var expected = Guid.NewGuid().ToString();
            var settings = new MemstateSettings("--Memstate:StorageProviders:Postgresql:Password", expected);
            var pgSettings = new PostgresqlSettings(settings);
            Assert.Equal(expected, pgSettings.Password);
        }

        [Fact]
        public void PasswordFromEnvironmentVariableOverridesConnectionString()
        {
            string key = "Memstate:StorageProviders:Postgresql:Password";
            var expected = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, expected);
            var settings = new MemstateSettings();
            var pgSettings = new PostgresqlSettings(settings);
            Assert.Equal(expected, pgSettings.Password);
            Environment.SetEnvironmentVariable(key, null);
        }
    }
}