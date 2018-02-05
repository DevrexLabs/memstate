namespace Memstate
{
    public class FileStorageSettings : Settings
    {
        public const string ConfigurationKey = "StorageProviders:FileStorage";

        public FileStorageSettings(MemstateSettings settings)
            : base(settings, ConfigurationKey)
        {
        }

        public string FileNameSuffix { get; set; } = ".journal";

        public string FileName => Memstate?.StreamName + FileNameSuffix;
    }
}