namespace Memstate.Postgresql
{
    public class PostgresqlSubscriptionSourceSettings : PostgresqlSettings
    {
        public PostgresqlSubscriptionSourceSettings()
        {
        }
        
        public PostgresqlSubscriptionSourceSettings(PostgresqlSettings settings)
        {
            CopyFrom(settings);
        }
    }
}