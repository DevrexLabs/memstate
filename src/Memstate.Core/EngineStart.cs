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
        /// <param name="config"></param>
        /// <param name="waitUntilReady"></param>
        /// <returns>A task that completes when the engine is ready to process messages</returns>
        public static async Task<Engine<T>> Start<T>(Config config = null, bool waitUntilReady = true) where T : class
        {
            config = config ?? Config.CreateDefault();
            var engine = Build<T>(config);
            await engine.Start(waitUntilReady);
            return engine;
        }

        public static async Task<Engine<T>> For<T>(Config config = null, bool waitUntilReady = true) where T : class
        {
            config = config ?? Config.CreateDefault();

            var container = config.Container;
            if (!container.CanResolve<Engine<T>>())
            {
                var engine = await Start<T>(config, waitUntilReady);
                container.Register(engine);
            }
            return container.Resolve<Engine<T>>();
        }
        
        /// <summary>
        /// Build but do not start
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Engine<T> Build<T>(Config config = null) where T : class
        {
            config = config ?? Config.CreateDefault();

            var modelType = typeof(T); 
            if (modelType.IsInterface)
                modelType = DeriveClassFromInterface(modelType);

            var initialState = (T) Activator.CreateInstance(modelType);
            return Build(initialState, config);
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
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Engine<T> Build<T>(T initialState, Config config = null) where T : class
        {
            config = config ?? Config.CreateDefault();
            return new Engine<T>(initialState, config);
        }

        public static async Task<Engine<T>> Start<T>(T model, Config config = null, bool waitUntilReady = true) where T : class
        {
            config = config ?? Config.CreateDefault();
            var engine = new Engine<T>(model, config);
            await engine.Start(waitUntilReady);
            return engine;
        }
    }
}