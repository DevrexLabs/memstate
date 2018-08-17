using Memstate.Configuration;

namespace Memstate
{
    public class FileStorageSettings : Settings
    {
        public override string Key { get;  } = "Memstate:StorageProviders:FileStorage";

        private readonly MemstateSettings _memstateSettings;

        public FileStorageSettings()
        {
            _memstateSettings = Config.Current.Resolve<MemstateSettings>();
        }

        public string FileNameSuffix { get; set; } = ".journal";

        public string FileName => _memstateSettings.StreamName + FileNameSuffix;
    }
}