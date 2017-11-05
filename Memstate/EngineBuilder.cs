namespace Memstate
{
    using System.Threading.Tasks;

    public class EngineBuilder
    {
        private readonly MemstateSettings _settings;
        private readonly StorageProvider _storageProvider;

        public EngineBuilder(MemstateSettings settings, StorageProvider storageProvider = null)
        {
            _settings = settings;
            _storageProvider = storageProvider ?? settings.CreateStorageProvider();
            
            // TODO: Figure out what to do if initialization has already been executed.
            _storageProvider.Initialize();
        }

        public Task<Engine<T>> BuildAsync<T>() where T : class, new()
        {
            return BuildAsync(new T());
        }

        public async Task<Engine<T>> BuildAsync<T>(T initialModel) where T : class
        {
            var reader = _storageProvider.CreateJournalReader();
            var loader = new ModelLoader();
            var model = loader.Load(reader, initialModel);
            var nextRecordNumber = loader.LastRecordNumber + 1;
            await reader.DisposeAsync().ConfigureAwait(false);
            var writer = _storageProvider.CreateJournalWriter(nextRecordNumber);
            var subscriptionSource = _storageProvider.CreateJournalSubscriptionSource();
            return new Engine<T>(_settings, model, subscriptionSource, writer, nextRecordNumber);
        }

        public Engine<T> Build<T>(T initialModel) where T : class
        {
            return BuildAsync(initialModel).Result;
        }

        public Engine<T> Build<T>() where T : class, new()
        {
            return BuildAsync(new T()).Result;
        }

    }
}