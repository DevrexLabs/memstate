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
            Type modelType = typeof(T); 
            if (modelType.IsInterface)
                modelType = DeriveClassFromInterface(modelType);

            var initialState = (T) Activator.CreateInstance(modelType);
            return Build(initialState);
        }

        internal static Type DeriveClassFromInterface(Type interfaceType)
        {
            var interfaceName = interfaceType.AssemblyQualifiedName;
            var idx = interfaceName.LastIndexOf(".I");

            //Inner interfaces will have a plus sign instead of dot
            if (idx == -1) idx = interfaceName.LastIndexOf("+I");

            var className = interfaceName.Remove(idx + 1, 1);

            var type = Type.GetType(className, throwOnError: true);
            return type;
        }

        public async Task<Engine<T>> Build<T>(T initialState) where T : class
        {
            var reader = _storageProvider.CreateJournalReader();
            var model = Load(reader, initialState, out long lastRecordNumber);
            await reader.DisposeAsync().ConfigureAwait(false);                                                                     
            return new Engine<T>(_settings, model, lastRecordNumber);
        }

        internal static TState Load<TState>(IJournalReader reader, TState initial, out long lastRecordNumber)
        {
            lastRecordNumber = -1;
            foreach (var journalRecord in reader.GetRecords())
            {
                try
                {
                    journalRecord.Command.ExecuteImpl(initial);
                    lastRecordNumber = journalRecord.RecordNumber;
                }
                catch
                {
                    // ignored
                }
            }
            return initial;
        }
    }
}