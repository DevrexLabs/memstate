namespace Memstate.Postgresql
{
    public static class MemstateSettingsExtensions
    {
        public static MemstateSettings UsePostgresqlProvider(this MemstateSettings settings)
        {
            settings.StorageProviderName = typeof(PostgresProvider).AssemblyQualifiedName;
            return settings;
        }
    }
}