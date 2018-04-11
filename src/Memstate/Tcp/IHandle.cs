namespace Memstate.Tcp
{
    internal interface IHandle<in T>
    {
        void Handle(T message);
    }
}