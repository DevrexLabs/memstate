namespace Memstate
{
    public class FileStorageSettings : Settings
    {
        public FileStorageSettings() 
            : base("Memstate.StorageProviders.FileStorage")
        {
        }

        public string FileNameSuffix { get; set; } = ".journal";

        /// <summary>
        /// Default Uses EngineSettings.StreamName
        /// </summary>
        public string FileNameWithoutSuffix { get; set; }
    }
}