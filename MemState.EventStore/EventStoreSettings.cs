using System;
using Microsoft.Extensions.Configuration;

namespace Memstate.EventStore
{
    public class EventStoreSettings : MemstateSettings
    {
        public const string ConfigurationKey = "StorageProviders:EventStore";

        public EventStoreSettings(IConfiguration configuration)
            : base(configuration.GetSection(ConfigurationKey))
        {
        }

        public EventStoreSettings(MemstateSettings settings)
            : this(settings.Configuration)
        {
        }

        public string ConnectionString { get; set; } = "ConnectTo=tcp://admin:changeit@localhost:1113";

        public override void Validate()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException("Property must have a value.", nameof(ConnectionString));
            }
        }
    }
}