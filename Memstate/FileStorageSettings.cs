namespace Memstate
{
    using Microsoft.Extensions.Configuration;

    public class FileStorageSettings : MemstateSettings
    {
        public const string ConfigurationKey = "StorageProviders:FileStorage";

        public FileStorageSettings(MemstateSettings parent)
            : this(parent.Configuration)
        {
        }

        public FileStorageSettings(IConfiguration configuration)
            : base(configuration.GetSection(ConfigurationKey))
        {
        }

        public string FileName { get; set; } = "Memstate.journal";
    }
}