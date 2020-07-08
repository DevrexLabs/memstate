using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public static class Engine
    {
        /// <summary>
        /// Load an existing or create a new engine
        /// </summary>
        /// <returns>A task that completes when the engine is ready to process messages</returns>
        public static async Task<Engine<T>> Start<T>() where T : class
        {
            var engine = Build<T>();
            await engine.Start();
            return engine;
        }

        public static async Task<Engine<T>> For<T>() where T : class
        {
            var container = Config.Current.Container;
            if (!container.CanResolve<Engine<T>>())
            {
                var engine = await Start<T>();
                container.Register(engine);
            }
            return container.Resolve<Engine<T>>();
        }
        
        /// <summary>
        /// Build but do not start
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Engine<T> Build<T>() where T : class
        {
            Type modelType = typeof(T); 
            if (modelType.IsInterface)
                modelType = DeriveClassFromInterface(modelType);

            var initialState = (T) Activator.CreateInstance(modelType);
            return Build(initialState);
        }

        [SuppressMessage("ReSharper", "StringLastIndexOfIsCultureSpecific.1")]
        internal static Type DeriveClassFromInterface(Type interfaceType)
        {
            var interfaceName = interfaceType.AssemblyQualifiedName;
            if (interfaceName is null) throw new Exception();
            var idx = interfaceName.LastIndexOf(".I");

            //Inner interfaces will have a plus sign instead of dot
            if (idx == -1) idx = interfaceName.LastIndexOf("+I");

            var className = interfaceName.Remove(idx + 1, 1);

            var type = Type.GetType(className, throwOnError: true);
            return type;
        }

        
        /// <summary>
        /// Build but do not start
        /// </summary>
        /// <param name="initialState"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Engine<T> Build<T>(T initialState) where T : class
        {
            var config = Config.Current;
            var engineSettings = config.GetSettings<EngineSettings>();
            var storageProvider = config.GetStorageProvider();
            //todo: push this whole method into into Engine class
            storageProvider.Provision().GetAwaiter().GetResult();
            return new Engine<T>(initialState, engineSettings, storageProvider);
        }
    }
}