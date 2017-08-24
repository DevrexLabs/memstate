namespace Memstate
{
    public interface IEngineBuilder
    {
        Engine<T> Build<T>() where T : class, new();
        Engine<T> Build<T>(T initialModel) where T : class;
    }
}