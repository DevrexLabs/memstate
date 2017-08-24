namespace Memstate.Postgresql
{
    public class PostgresqlWriterSettings : PostgresqlSettings
    {
        public PostgresqlWriterSettings()
        {
        }
        
        public PostgresqlWriterSettings(PostgresqlSettings settings)
        {
            CopyFrom(settings);
        }
        
        public ISerializer Serializer { get; set; }
    }
}