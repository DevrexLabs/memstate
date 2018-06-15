using System.Threading.Tasks;

namespace Memstate
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}