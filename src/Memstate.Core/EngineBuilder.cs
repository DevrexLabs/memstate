using System;
using Memstate.Configuration;

namespace Memstate
{
    public class EngineBuilder
    {
        private readonly EngineSettings _settings;
        private readonly IStorageProvider _storageProvider;

        public EngineBuilder()
        {
            var config = Config.Current;
            _settings = config.GetSettings<EngineSettings>();
            _storageProvider = config.GetStorageProvider();
            _storageProvider.Provision().GetAwaiter().GetResult();
        }

        public Engine<T> Build<T>() where T : class
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

        public Engine<T> Build<T>(T initialState) where T : class
        {
            return new Engine<T>(initialState, _settings, _storageProvider);
        }
    }
}