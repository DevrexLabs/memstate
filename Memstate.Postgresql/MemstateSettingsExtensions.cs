namespace Memstate.Postgresql
{
    public static class MemstateSettingsExtensions
    {
        public static MemstateSettings UsePostgresqlProvider(this MemstateSettings settings)
        {
            settings.StorageProvider = typeof(PostgresqlProvider).AssemblyQualifiedName;
            
            return settings;
        }
    }
}