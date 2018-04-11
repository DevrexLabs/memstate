namespace Memstate
{
    public class StorageProviders : Providers<StorageProvider>
    {
        public StorageProviders()
        {
            Register("file", settings => new FileStorageProvider(settings));
        }
    }
}