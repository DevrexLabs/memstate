namespace Memstate.Postgresql
{
    public class PostgresqlSettings
    {
        public string ConnectionString { get; set; }
        
        public string Table { get; set; } = "commands";

        public string SubscriptionStream { get; set; } = "commands_stream";

        protected void CopyFrom(PostgresqlSettings settings)
        {
            ConnectionString = settings.ConnectionString;
            Table = settings.Table;
        }

        public T Upgrade<T>() where T : PostgresqlSettings, new()
        {
            var settings = new T();

            settings.CopyFrom(this);

            return settings;
        }
    }
}