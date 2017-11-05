namespace Memstate
{
    using System.Threading.Tasks;

    public static class Engine
    {
        public static Task<Engine<T>> StartAsync<T>(MemstateSettings settings = null) where T : class, new()
        {
            settings = settings ?? new MemstateSettings();
            return new EngineBuilder(settings).BuildAsync<T>();
        }
    }
}