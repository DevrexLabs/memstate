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

        public string FileName => Memstate?.StreamName + "_{0}" + FileNameSuffix;

        public int PageSize { get; set; } = 65536;
    }
}