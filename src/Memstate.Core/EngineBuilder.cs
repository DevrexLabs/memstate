using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class EngineBuilder
    {
        private readonly EngineSettings _settings;
        private readonly StorageProvider _storageProvider;

        public EngineBuilder()
        {
            var config = Config.Current;
            _settings = config.GetSettings<EngineSettings>();
            _storageProvider = config.GetStorageProvider();
            _storageProvider.Initialize();
        }

        public Task<Engine<T>> Build<T>() where T : class
        {
            var model = typeof(T).IsInterface
                            ? CreateInstance<T>()
                            : Activator.CreateInstance<T>();
            return Build(model);
        }

        private T CreateInstance<T>()
        {
            var interfaceName = typeof(T).AssemblyQualifiedName;
            var idx = interfaceName.LastIndexOf(".I");

            //Inner interfaces will have a plus sign instead of dot
            if (idx == -1) idx = interfaceName.LastIndexOf("+I");

            var className = interfaceName.Remove(idx + 1, 1);

            var type = Type.GetType(className);
            return (T) Activator.CreateInstance(type);
        }

        public async Task<Engine<T>> Build<T>(T initialModel) where T : class
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
    }
}