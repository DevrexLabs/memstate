namespace Memstate.Core
{
    public interface IJournalWriter
    {
        void Send(Command command);
        void Dispose();
    }
}