namespace Memstate
{
    public class EngineBuilder
    {
        private readonly Settings _config;
        private readonly StorageProvider _storageProvider;

        public EngineBuilder(Settings settings, StorageProvider storageProvider = null)
        {
            _config = settings;
            _storageProvider = storageProvider ?? settings.CreateStorageProvider();
        }

        public Engine<T> Build<T>() where T : class, new()
        {
            return Build(new T());
        }

        public Engine<T> Build<T>(T initialModel) where T : class
        {

            var reader = _storageProvider.CreateJournalReader();
            var loader = new ModelLoader();
            var model = loader.Load(reader, initialModel);
            var nextRecordNumber = loader.LastRecordNumber + 1;
            var writer = _storageProvider.CreateJournalWriter(nextRecordNumber);
            var subscriptionSource = _storageProvider.CreateJournalSubscriptionSource();
            return new Engine<T>(_config, model, subscriptionSource, writer, nextRecordNumber);
        }
    }
}