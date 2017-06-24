namespace Memstate.Core
{
    public interface IHandle<in T>
    {
        void Handle(T item);
    }
}