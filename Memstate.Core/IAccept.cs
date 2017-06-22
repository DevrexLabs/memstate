namespace Memstate.Core
{
    public interface IAccept<in T>
    {
        void Accept(T item);
    }
}