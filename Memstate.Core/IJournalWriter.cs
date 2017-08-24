namespace Memstate
{
    public interface IJournalWriter
    {
        void Send(Command command);
        void Dispose();
    }
}