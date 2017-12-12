using Xunit;
using Xunit.Abstractions;

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
            Assert.Equal(PostgresqlSettings.DefaultConnectionString, _settings.ConnectionString);
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
    }
}