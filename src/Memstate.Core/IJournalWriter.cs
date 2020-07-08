using System.Threading.Tasks;

namespace Memstate
{
    public interface IJournalWriter : IAsyncDisposable
    {
        Task Write(Command command);
    }
}