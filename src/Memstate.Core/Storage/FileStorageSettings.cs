using Memstate.Configuration;

namespace Memstate
{
    public class FileStorageSettings : Settings
    {
        

        private readonly EngineSettings _memstateSettings;

        public FileStorageSettings() 
            : base("Memstate.StorageProviders.FileStorage")
        {
            _memstateSettings = Config.Current.GetSettings<EngineSettings>();
        }

        public string FileNameSuffix { get; set; } = ".journal";

        public string FileName => _memstateSettings.StreamName + FileNameSuffix;
    }
}