namespace Memstate.Core
{
    public interface IJournalWriter
    {
        void AppendAsync(Command command);
        void Dispose();
    }
}