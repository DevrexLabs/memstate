namespace Memstate
{
    public class FileStorageSettings : Settings
    {
        public const string ConfigurationKey = "StorageProviders:FileStorage";

        public FileStorageSettings(MemstateSettings settings) : base(settings, ConfigurationKey)
        {
        }

        public string FileName { get; set; } = "Memstate.journal";
    }
}