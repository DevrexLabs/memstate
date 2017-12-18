namespace Memstate
{
    public interface IModelCreator
    {
        T Create<T>();
    }
}