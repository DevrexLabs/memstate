namespace Memstate
{
    public class StorageProviders : Providers<StorageProvider>
    {
        public StorageProviders()
        {
            Register("file", settings => new FileStorageProvider(settings));
            Register("inmemory", settings => new InMemoryStorageProvider(settings));
        }
    }
}