namespace Memstate
{
    public interface IJournalWriter : IAsyncDisposable
    {
        void Send(Command command);
    }
}