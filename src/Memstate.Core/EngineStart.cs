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
            //todo: polish the API
            var engine = new EngineBuilder().Build<T>();
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
    }
}