using System.Threading.Tasks;

namespace Memstate.Tcp
{
    internal interface IHandle<in T>
    {
        Task Handle(T message);
    }
}