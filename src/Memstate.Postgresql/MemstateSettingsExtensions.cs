using Memstate.Configuration;

namespace Memstate.Postgres
{
    public static class MemstateSettingsExtensions
    {
        public static Config UsePostgresqlProvider(this Config config)
        {
            config.StorageProviderName = typeof(PostgresProvider).AssemblyQualifiedName;
            return config;
        }
    }
}