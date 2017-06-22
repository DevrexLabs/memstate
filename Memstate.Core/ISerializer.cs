namespace Memstate.Core
{
    public interface ISerializer<T>
    {
        byte[] Serialize(T graph);
        T Deserialize(byte[] data);
    }
}