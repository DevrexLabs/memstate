namespace Memstate.Postgresql.Tests
{
    using Xunit;

    public class PostgresqlSettingsTests
    {
        private readonly PostgresqlSettings _settings;
        private readonly MemstateSettings _memstateSettings;

        public PostgresqlSettingsTests()
        {
            _memstateSettings = new MemstateSettings();
            _settings = new PostgresqlSettings(_memstateSettings);
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