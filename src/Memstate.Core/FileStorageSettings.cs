namespace Memstate
{
    public class FileStorageSettings : Settings
    {
        public override string Key { get;  } = "Memstate:StorageProviders:FileStorage";

        private readonly MemstateSettings _memstateSettings;

        public FileStorageSettings(MemstateSettings settings)
        {
            Ensure.NotNull(settings, nameof(settings));
            _memstateSettings = settings;
        }

        public string FileNameSuffix { get; set; } = ".journal";

        public string FileName => _memstateSettings.StreamName + FileNameSuffix;
    }
}