using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class EngineBuilder
    {
        private readonly EngineSettings _settings;
        private readonly StorageProvider _storageProvider;
        private readonly IStorage _storage;

        public EngineBuilder(Config config = null)
        {
            config ??= new Config();
            _settings = config.GetSettings<EngineSettings>();
            var serializer = config.CreateSerializer();
            _storage = new FileStorage(new HostFileSystem(), _settings.StreamName, serializer);
            //_storageProvider = config.GetStorageProvider();
            //_storageProvider.Initialize();
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
            var (model, lastRecordNumber) = await Load(_storage, initialState);
            return new Engine<T>(_storage, _settings, model, lastRecordNumber);
        }

        internal static async Task<(TModel, long)> Load<TModel>(IStorage storage, TModel initial)
        {
            long lastRecordNumber = 0;
            await foreach (var chunk in storage.ReadRecords(1) )
            {
                foreach (var record in chunk)
                {
                    try
                    {
                        record.Command.ExecuteImpl(initial);
                        lastRecordNumber = record.RecordNumber;
                    }
                    catch
                    {
                        // ignored
                    }
                    
                }
            }
            return (initial, lastRecordNumber);
        }
    }
}