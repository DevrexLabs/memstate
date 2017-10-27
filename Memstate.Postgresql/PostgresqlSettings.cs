using System;

namespace Memstate.Postgresql
{
    public class PostgresqlSettings
    {
        public string ConnectionString { get; set; }
        
        public string Table { get; set; } = "commands";

        public string SubscriptionStream { get; set; } = "commands_stream";


        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentException("Property must have a value.", nameof(ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(Table))
            {
                throw new ArgumentException("Property must have a value.", nameof(Table));
            }

            if (string.IsNullOrWhiteSpace(SubscriptionStream))
            {
                throw new ArgumentException("Property must have a value.", nameof(Table));
            }

        }
    }
}