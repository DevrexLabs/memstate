using System.Threading.Tasks;

namespace Memstate
{
    public static class Engine
    {
        public static Task<Engine<T>> Start<T>(MemstateSettings settings = null) where T : class, new()
        {
            settings = settings ?? new MemstateSettings();
            return new EngineBuilder(settings).Build<T>();
        }
    }
}